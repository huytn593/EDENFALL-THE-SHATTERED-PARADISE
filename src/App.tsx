import React, { useState, useEffect, useCallback } from 'react';
import { GameState, DialogueChoice, Item, MapLocation, CinematicScene, VisionLayer, TimeOfDay, CharacterRelationship } from './types';
import { MASTER_CHARACTERS } from './data/characters';
import { INITIAL_ITEMS, INITIAL_QUESTS } from './data/initialGameData';
import { CINEMATIC_SCENES } from './data/cinematics';
import { ExplorationCanvas } from './components/ExplorationCanvas';
import { DialogueMode } from './components/DialogueMode';
import { CinematicEventModal } from './components/CinematicEventModal';
import { RelationshipJournal } from './components/RelationshipJournal';
import { InventoryModal } from './components/InventoryModal';
import { QuestJournal } from './components/QuestJournal';
import { GalleryModal } from './components/GalleryModal';
import { SaveLoadModal } from './components/SaveLoadModal';
import { LayeredVisionSelector } from './components/LayeredVisionSelector';
import { WorldStatusHeader } from './components/WorldStatusHeader';
import { sound } from './utils/audio';
import { Sparkles, MessageCircle, MapPin, Check, Info } from 'lucide-react';

export const App: React.FC = () => {
  // Initialize Master Game State
  const [gameState, setGameState] = useState<GameState>(() => {
    try {
      const saved = localStorage.getItem('edenfall_current_autosave');
      if (saved) return JSON.parse(saved);
    } catch {
      // ignore
    }
    return {
      player: {
        name: 'Echo',
        title: 'Awakened Seeker',
        x: 500,
        y: 350,
        stamina: 100,
        maxStamina: 100,
        luminaryResonance: 40
      },
      world: {
        timeOfDay: 'Morning',
        dayNumber: 1,
        weather: 'Celestial Clear',
        currentRegion: 'Dawn Sanctuary',
        currentLocationId: 'loc_spire',
        activeVisionLayer: 'normal',
        sanctuaryRestoration: 35
      },
      relationships: MASTER_CHARACTERS,
      inventory: INITIAL_ITEMS,
      quests: INITIAL_QUESTS,
      unlockedCGs: ['cg_aurelia_garden'],
      activeCinematic: null,
      activeDialogue: null,
      logs: [
        {
          id: 'log_init',
          time: 'Day 1 - Morning',
          text: 'The Echo awakens in Dawn Sanctuary. Aurelia’s gentle voice echoes across the ruins.',
          type: 'event'
        }
      ]
    };
  });

  const [activeTab, setActiveTab] = useState<'explore' | 'relationships' | 'inventory' | 'quests' | 'gallery'>('explore');
  const [showSaveModal, setShowSaveModal] = useState(false);
  const [activeDialogueCharacter, setActiveDialogueCharacter] = useState<CharacterRelationship | null>(null);
  const [activeCinematicScene, setActiveCinematicScene] = useState<CinematicScene | null>(null);
  const [isMuted, setIsMuted] = useState(false);
  const [selectedLocationInfo, setSelectedLocationInfo] = useState<MapLocation | null>(null);

  // Auto-save to localStorage
  useEffect(() => {
    try {
      localStorage.setItem('edenfall_current_autosave', JSON.stringify(gameState));
    } catch {
      // ignore
    }
  }, [gameState]);

  // Handle Player Movement & Stamina updates
  const handleUpdatePlayerPos = useCallback((x: number, y: number, staminaDelta: number) => {
    setGameState(prev => ({
      ...prev,
      player: {
        ...prev.player,
        x,
        y,
        stamina: Math.max(0, Math.min(prev.player.maxStamina, prev.player.stamina + staminaDelta))
      }
    }));
  }, []);

  // Handle Location click
  const handleSelectLocation = (loc: MapLocation) => {
    setSelectedLocationInfo(loc);
  };

  // Handle Interaction with Node or NPC
  const handleInteractNode = (nodeId: string) => {
    // Check if it matches an NPC
    const char = gameState.relationships[nodeId];
    if (char) {
      sound.playClick();
      setActiveDialogueCharacter(char);
      return;
    }

    // Special node: Spire core
    if (nodeId === 'loc_spire' || nodeId === 'node_spire') {
      sound.playCrystalChime();
      setActiveCinematicScene(CINEMATIC_SCENES['cg_spire_awakening']);
      return;
    }

    // Default: find character nearest or open location info
    setActiveDialogueCharacter(gameState.relationships['aurelia']);
  };

  // Handle Dialogue Choice Selection
  const handleChoiceSelect = (choice: DialogueChoice) => {
    if (!activeDialogueCharacter) return;
    const charId = activeDialogueCharacter.id;

    setGameState(prev => {
      const char = prev.relationships[charId];
      if (!char) return prev;

      const newAffection = Math.min(100, Math.max(0, char.affection + (choice.affectionDelta || 0)));
      const newTrust = Math.min(100, Math.max(0, char.trust + (choice.trustDelta || 0)));
      const newRespect = Math.min(100, Math.max(0, char.respect + (choice.respectDelta || 0)));
      const newComfort = Math.min(100, Math.max(0, char.comfort + (choice.comfortDelta || 0)));
      const newConflict = Math.min(100, Math.max(0, char.conflict + (choice.conflictDelta || 0)));

      // Determine new stage
      let stage = char.stage;
      if (newAffection >= 70 && newTrust >= 60) stage = 'Romantic Possibility';
      else if (newAffection >= 50 && newTrust >= 40) stage = 'Emotional Connection';
      else if (newAffection >= 30) stage = 'Trusted Companion';
      else if (newAffection >= 15) stage = 'Acquaintance';

      const newMemories = [...char.memories];
      if (choice.memoryText) {
        newMemories.push({
          id: `mem_${Date.now()}`,
          text: choice.memoryText,
          timestamp: `Day ${prev.world.dayNumber} - ${prev.world.timeOfDay}`,
          impact: choice.tone === 'romantic' ? 'romantic' : choice.affectionDelta && choice.affectionDelta > 0 ? 'positive' : 'neutral'
        });
      }

      // Check quest step completion
      const updatedQuests = prev.quests.map(q => {
        if (choice.questTrigger && q.id === choice.questTrigger.questId) {
          const newSteps = [...q.steps];
          if (newSteps[choice.questTrigger.stepIndex]) {
            newSteps[choice.questTrigger.stepIndex].completed = true;
          }
          const allDone = newSteps.every(s => s.completed);
          return {
            ...q,
            steps: newSteps,
            status: allDone ? ('completed' as const) : q.status
          };
        }
        return q;
      });

      return {
        ...prev,
        relationships: {
          ...prev.relationships,
          [charId]: {
            ...char,
            affection: newAffection,
            trust: newTrust,
            respect: newRespect,
            comfort: newComfort,
            conflict: newConflict,
            stage,
            memories: newMemories
          }
        },
        quests: updatedQuests,
        world: {
          ...prev.world,
          sanctuaryRestoration: Math.min(100, prev.world.sanctuaryRestoration + 3)
        },
        logs: [
          ...prev.logs,
          {
            id: `log_${Date.now()}`,
            time: `Day ${prev.world.dayNumber} - ${prev.world.timeOfDay}`,
            text: `Interacted with ${char.name}: ${choice.memoryText || 'Deepened bond.'}`,
            type: 'relationship'
          }
        ]
      };
    });

    // Refresh active dialogue character instance
    setActiveDialogueCharacter(prev => {
      if (!prev) return null;
      return {
        ...prev,
        affection: Math.min(100, Math.max(0, prev.affection + (choice.affectionDelta || 0))),
        trust: Math.min(100, Math.max(0, prev.trust + (choice.trustDelta || 0)))
      };
    });
  };

  // Handle Gifting Item
  const handleGiftItem = (characterId: string, item: Item) => {
    setGameState(prev => {
      const char = prev.relationships[characterId];
      if (!char) return prev;

      const isFavorite = char.favoriteGifts.includes(item.id);
      const affBonus = isFavorite ? 20 : 10;
      const trustBonus = isFavorite ? 15 : 8;

      // Decrement item quantity
      const newInventory = prev.inventory
        .map(i => (i.id === item.id ? { ...i, quantity: i.quantity - 1 } : i))
        .filter(i => i.quantity > 0);

      const newMemories = [
        ...char.memories,
        {
          id: `gift_${Date.now()}`,
          text: `Gratefully accepted ${item.name}${isFavorite ? ' (Favorite Gift!)' : ''}.`,
          timestamp: `Day ${prev.world.dayNumber} - ${prev.world.timeOfDay}`,
          impact: 'positive' as const
        }
      ];

      return {
        ...prev,
        inventory: newInventory,
        relationships: {
          ...prev.relationships,
          [characterId]: {
            ...char,
            affection: Math.min(100, char.affection + affBonus),
            trust: Math.min(100, char.trust + trustBonus),
            memories: newMemories
          }
        },
        logs: [
          ...prev.logs,
          {
            id: `log_${Date.now()}`,
            time: `Day ${prev.world.dayNumber} - ${prev.world.timeOfDay}`,
            text: `Gifted ${item.name} to ${char.name}. Affection +${affBonus}, Trust +${trustBonus}!`,
            type: 'item'
          }
        ]
      };
    });
  };

  // Handle Using an Item
  const handleUseItem = (item: Item) => {
    setGameState(prev => {
      let staminaPlus = 0;
      let resonancePlus = 0;

      if (item.id === 'astral_draught' || item.id === 'astral_draught_gift') staminaPlus = 40;
      else if (item.id === 'astral_dew') {
        staminaPlus = 100;
        resonancePlus = 15;
      } else if (item.id === 'luminary_seal') {
        resonancePlus = 20;
      }

      const newInventory = prev.inventory
        .map(i => (i.id === item.id ? { ...i, quantity: i.quantity - 1 } : i))
        .filter(i => i.quantity > 0);

      return {
        ...prev,
        player: {
          ...prev.player,
          stamina: Math.min(prev.player.maxStamina, prev.player.stamina + staminaPlus),
          luminaryResonance: Math.min(100, prev.player.luminaryResonance + resonancePlus)
        },
        inventory: newInventory,
        logs: [
          ...prev.logs,
          {
            id: `log_${Date.now()}`,
            time: `Day ${prev.world.dayNumber} - ${prev.world.timeOfDay}`,
            text: `Activated ${item.name}. ${item.effectDescription || ''}`,
            type: 'item'
          }
        ]
      };
    });
  };

  // Advance Time of Day
  const handleAdvanceTime = () => {
    const timeCycle: Record<TimeOfDay, TimeOfDay> = {
      Morning: 'Afternoon',
      Afternoon: 'Evening',
      Evening: 'Night',
      Night: 'Morning'
    };

    setGameState(prev => {
      const nextTime = timeCycle[prev.world.timeOfDay];
      const nextDay = nextTime === 'Morning' ? prev.world.dayNumber + 1 : prev.world.dayNumber;

      return {
        ...prev,
        player: {
          ...prev.player,
          stamina: prev.player.maxStamina // Recover stamina upon resting/advancing
        },
        world: {
          ...prev.world,
          timeOfDay: nextTime,
          dayNumber: nextDay
        },
        logs: [
          ...prev.logs,
          {
            id: `time_${Date.now()}`,
            time: `Day ${nextDay} - ${nextTime}`,
            text: `Time advanced to ${nextTime}. Inhabitants have moved to their contextual locations.`,
            type: 'event'
          }
        ]
      };
    });
  };

  // Change Vision Layer
  const handleChangeVisionLayer = (layer: VisionLayer) => {
    setGameState(prev => ({
      ...prev,
      world: {
        ...prev.world,
        activeVisionLayer: layer
      }
    }));
  };

  // Complete Cinematic
  const handleCompleteCinematic = (scene: CinematicScene) => {
    setGameState(prev => {
      const char = prev.relationships[scene.characterId];
      const updatedAff = char ? Math.min(100, char.affection + scene.rewards.affectionBonus) : 0;
      const updatedTrust = char ? Math.min(100, char.trust + scene.rewards.trustBonus) : 0;

      const newUnlocked = Array.from(new Set([...prev.unlockedCGs, scene.id]));

      return {
        ...prev,
        unlockedCGs: newUnlocked,
        relationships: char
          ? {
              ...prev.relationships,
              [scene.characterId]: {
                ...char,
                affection: updatedAff,
                trust: updatedTrust,
                memories: [
                  ...char.memories,
                  {
                    id: `cg_mem_${Date.now()}`,
                    text: scene.rewards.memoryText,
                    timestamp: `Day ${prev.world.dayNumber} - ${prev.world.timeOfDay}`,
                    impact: 'romantic'
                  }
                ]
              }
            }
          : prev.relationships,
        world: {
          ...prev.world,
          sanctuaryRestoration: Math.min(100, prev.world.sanctuaryRestoration + 8)
        },
        logs: [
          ...prev.logs,
          {
            id: `cg_log_${Date.now()}`,
            time: `Day ${prev.world.dayNumber} - ${prev.world.timeOfDay}`,
            text: `Concluded Cinematic Memory: “${scene.title}”. Memory permanently recorded in Gallery.`,
            type: 'event'
          }
        ]
      };
    });

    setActiveCinematicScene(null);
  };

  // Toggle Sound
  const handleToggleSound = () => {
    const muted = sound.toggleMute();
    setIsMuted(muted);
  };

  // First user interaction starts ambience
  useEffect(() => {
    const handleFirstGesture = () => {
      sound.startAmbience();
      window.removeEventListener('click', handleFirstGesture);
    };
    window.addEventListener('click', handleFirstGesture);
    return () => window.removeEventListener('click', handleFirstGesture);
  }, []);

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col font-sans select-none overflow-x-hidden">
      {/* Top World & Status Header */}
      <WorldStatusHeader
        gameState={gameState}
        activeTab={activeTab}
        isMuted={isMuted}
        onTabChange={setActiveTab}
        onAdvanceTime={handleAdvanceTime}
        onToggleSound={handleToggleSound}
        onOpenSave={() => setShowSaveModal(true)}
      />

      {/* Main Content Stage */}
      <main className="flex-1 max-w-7xl w-full mx-auto p-3 sm:p-4 md:p-6 flex flex-col gap-4">
        {/* Layered Vision Bar & Quick Context */}
        <div className="flex flex-col sm:flex-row items-center justify-between gap-3">
          <LayeredVisionSelector
            currentLayer={gameState.world.activeVisionLayer}
            onChangeLayer={handleChangeVisionLayer}
          />

          <div className="text-xs text-slate-400 flex items-center gap-2">
            <MapPin size={13} className="text-amber-400" />
            <span>Region: <span className="text-slate-200 font-semibold">{gameState.world.currentRegion}</span></span>
            <span>• Weather: <span className="text-sky-300">{gameState.world.weather}</span></span>
          </div>
        </div>

        {/* Dynamic Tab Views */}
        {activeTab === 'explore' && (
          <div className="space-y-4">
            <ExplorationCanvas
              gameState={gameState}
              onSelectLocation={handleSelectLocation}
              onInteractNode={handleInteractNode}
              onUpdatePlayerPos={handleUpdatePlayerPos}
            />

            {/* Quick Inhabitant Bar */}
            <div className="p-4 bg-slate-900/80 rounded-2xl border border-slate-800 shadow-md">
              <div className="flex items-center justify-between mb-3">
                <h3 className="font-cinzel text-xs font-bold text-amber-300 uppercase tracking-wider flex items-center gap-1.5">
                  <Sparkles size={14} /> Sanctuary Inhabitants Active Now ({gameState.world.timeOfDay})
                </h3>
                <span className="text-[11px] text-slate-400">Click any companion to initiate dialogue</span>
              </div>

              <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
                {Object.values(gameState.relationships).map(char => (
                  <button
                    key={char.id}
                    onClick={() => {
                      sound.playClick();
                      setActiveDialogueCharacter(char);
                    }}
                    className="flex items-center gap-3 p-2.5 rounded-xl bg-slate-950/70 hover:bg-amber-950/30 border border-slate-800 hover:border-amber-500/50 text-left transition group"
                  >
                    <span className="text-2xl group-hover:scale-110 transition">{char.portrait}</span>
                    <div className="min-w-0">
                      <div className="font-cinzel text-xs font-bold text-slate-200 truncate group-hover:text-amber-300">
                        {char.name}
                      </div>
                      <div className="text-[10px] text-slate-400 truncate">
                        {char.schedules[gameState.world.timeOfDay].activity}
                      </div>
                    </div>
                  </button>
                ))}
              </div>
            </div>
          </div>
        )}

        {activeTab === 'relationships' && (
          <RelationshipJournal
            relationships={gameState.relationships}
            visionLayer={gameState.world.activeVisionLayer}
            onSelectCharacter={char => setActiveDialogueCharacter(char)}
            onClose={() => setActiveTab('explore')}
          />
        )}

        {activeTab === 'inventory' && (
          <InventoryModal
            inventory={gameState.inventory}
            onUseItem={handleUseItem}
            onClose={() => setActiveTab('explore')}
          />
        )}

        {activeTab === 'quests' && (
          <QuestJournal
            quests={gameState.quests}
            onClose={() => setActiveTab('explore')}
          />
        )}

        {activeTab === 'gallery' && (
          <GalleryModal
            unlockedCGs={gameState.unlockedCGs}
            onPlayScene={scene => setActiveCinematicScene(scene)}
            onClose={() => setActiveTab('explore')}
          />
        )}

        {/* Location Inspector Modal */}
        {selectedLocationInfo && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm">
            <div className="bg-slate-900 border border-slate-700 rounded-3xl p-6 max-w-lg w-full shadow-2xl">
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2">
                  <span className="text-3xl">{selectedLocationInfo.icon}</span>
                  <div>
                    <h3 className="font-cinzel text-lg font-bold text-amber-200">{selectedLocationInfo.name}</h3>
                    <p className="text-xs text-slate-400">{selectedLocationInfo.subtitle}</p>
                  </div>
                </div>
                <button onClick={() => setSelectedLocationInfo(null)} className="text-slate-400 hover:text-white">✕</button>
              </div>

              <p className="text-xs text-slate-300 leading-relaxed mb-4">{selectedLocationInfo.description}</p>

              <div className="space-y-2 mb-5">
                <h4 className="text-xs font-bold text-slate-300 uppercase tracking-wider">Interactive Relics & Features:</h4>
                {selectedLocationInfo.interactiveObjects.map(obj => (
                  <div key={obj.id} className="p-2.5 rounded-xl bg-slate-950/60 border border-slate-800 text-xs flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <span>{obj.icon}</span>
                      <div>
                        <div className="font-semibold text-slate-200">{obj.name}</div>
                        <div className="text-[10px] text-slate-400">{obj.description}</div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>

              <div className="flex gap-2">
                <button
                  onClick={() => {
                    handleUpdatePlayerPos(selectedLocationInfo.x, selectedLocationInfo.y, 0);
                    setSelectedLocationInfo(null);
                    sound.playClick();
                  }}
                  className="flex-1 py-2.5 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs transition"
                >
                  Fast Travel to Location
                </button>
                <button
                  onClick={() => setSelectedLocationInfo(null)}
                  className="px-4 py-2.5 rounded-xl bg-slate-800 text-slate-300 font-semibold text-xs hover:bg-slate-700 transition"
                >
                  Close
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Live Chronicle Log Ticker */}
        <div className="bg-slate-900/60 border border-slate-800/80 rounded-xl px-4 py-2 text-xs text-slate-400 flex items-center justify-between">
          <div className="flex items-center gap-2 truncate">
            <span className="text-amber-400 font-semibold">Latest Chronicle:</span>
            <span className="text-slate-200 truncate">{gameState.logs[gameState.logs.length - 1]?.text}</span>
          </div>
          <span className="text-[10px] text-slate-500 font-mono flex-shrink-0 ml-4">
            {gameState.logs[gameState.logs.length - 1]?.time}
          </span>
        </div>
      </main>

      {/* Fullscreen Modals & Overlays */}
      {/* 1. Dialogue Mode */}
      {activeDialogueCharacter && (
        <DialogueMode
          character={activeDialogueCharacter}
          visionLayer={gameState.world.activeVisionLayer}
          inventory={gameState.inventory}
          onChoiceSelect={handleChoiceSelect}
          onGiftItem={handleGiftItem}
          onClose={() => setActiveDialogueCharacter(null)}
          onStartCinematic={sceneId => {
            const scene = CINEMATIC_SCENES[sceneId];
            if (scene) {
              setActiveDialogueCharacter(null);
              setActiveCinematicScene(scene);
            }
          }}
        />
      )}

      {/* 2. Cinematic Event Mode */}
      {activeCinematicScene && (
        <CinematicEventModal
          scene={activeCinematicScene}
          onComplete={handleCompleteCinematic}
          onClose={() => setActiveCinematicScene(null)}
        />
      )}

      {/* 3. Save / Load Modal */}
      {showSaveModal && (
        <SaveLoadModal
          currentGameState={gameState}
          onLoadGameState={newState => setGameState(newState)}
          onClose={() => setShowSaveModal(false)}
        />
      )}
    </div>
  );
};
