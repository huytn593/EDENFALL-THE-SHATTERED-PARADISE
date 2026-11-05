import React, { useState, useEffect } from 'react';
import { AnimePortrait } from './AnimePortrait';
import { CharacterRelationship, DialogueChoice, DialogueNode, Item, VisionLayer } from '../types';
import { sound } from '../utils/audio';
import { Heart, Shield, Sparkles, MessageSquare, Gift, BookOpen, Volume2, Eye, X } from 'lucide-react';

interface DialogueModeProps {
  character: CharacterRelationship;
  visionLayer: VisionLayer;
  inventory: Item[];
  onChoiceSelect: (choice: DialogueChoice) => void;
  onGiftItem: (characterId: string, item: Item) => void;
  onClose: () => void;
  onStartCinematic?: (sceneId: string) => void;
}

export const DialogueMode: React.FC<DialogueModeProps> = ({
  character,
  visionLayer,
  inventory,
  onChoiceSelect,
  onGiftItem,
  onClose,
  onStartCinematic
}) => {
  const [displayedText, setDisplayedText] = useState('');
  const [currentTextIndex, setCurrentTextIndex] = useState(0);
  const [showHistory, setShowHistory] = useState(false);
  const [showGiftModal, setShowGiftModal] = useState(false);
  const [historyLog, setHistoryLog] = useState<{ speaker: string; text: string }[]>([]);

  // Dialogues for Aurelia and characters with branching
  const isAurelia = character.id === 'aurelia';
  const isTeresa = character.id === 'teresa';
  const isVaelen = character.id === 'vaelen';
  const isSylvara = character.id === 'sylvara';

  // Determine current active node based on affection and relationship stage
  const getInitialNode = (): DialogueNode => {
    if (isAurelia) {
      if (character.affection >= 50) {
        return {
          id: 'aur_intimate',
          speaker: character.name,
          emotion: 'radiant',
          text: `“When you look at me like that, Echo... I feel as though the Shattered Paradise is not lost, but merely waiting for us to rebuild it together. My heart beats faster whenever you step into Dawn Garden.”`,
          choices: [
            {
              id: 'aur_c_romance',
              text: '“Then let us rebuild Eden together, Aurelia. Step closer.”',
              tone: 'romantic',
              affectionDelta: 8,
              comfortDelta: 10,
              triggerEventId: 'cg_aurelia_garden',
              memoryText: 'Shared a tender, intimate promise beneath the celestial blossoms.'
            },
            {
              id: 'aur_c_magic',
              text: '“Your Luminary Crest has awakened new resonance across the valley.”',
              tone: 'mystical',
              respectDelta: 6,
              trustDelta: 6
            },
            {
              id: 'aur_c_flirt',
              text: '“You are far more captivating than the starlight above us.”',
              tone: 'flirtatious',
              affectionDelta: 7,
              comfortDelta: 5
            }
          ]
        };
      } else {
        return {
          id: 'aur_start',
          speaker: character.name,
          emotion: 'gentle',
          text: `“Welcome to Dawn Sanctuary, outsider. My name is Aurelia. Few awaken with their faculties intact after traversing the celestial void. Are you hurt? Tell me what you remember before the fall.”`,
          choices: [
            {
              id: 'aur_c1',
              text: '“I remember only starlight and a melody... and then your voice.”',
              tone: 'compassionate',
              affectionDelta: 6,
              trustDelta: 5,
              memoryText: 'Spoke with profound vulnerability upon first waking.'
            },
            {
              id: 'aur_c2',
              text: '“Who are you to question an Echo? Explain what shattered this realm.”',
              tone: 'defiant',
              conflictDelta: 4,
              respectDelta: 3,
              memoryText: 'Exhibited fiery pride and independence.'
            },
            {
              id: 'aur_c3',
              text: '“I need to understand this sanctuary and the Eclipse Gate to the south.”',
              tone: 'inquisitive',
              trustDelta: 4,
              respectDelta: 4
            }
          ]
        };
      }
    } else if (isTeresa) {
      return {
        id: 'ter_start',
        speaker: character.name,
        emotion: 'serious',
        text: `“You stand before the Vanguard Bastion, Echo. My sentries have held this wall against entropic fiends for three centuries. If you carry corruption or reckless magic, I will strike you down myself.”`,
        choices: [
          {
            id: 'ter_c1',
            text: '“I carry no malice, Commander. I offer my strength to stand beside your soldiers.”',
            tone: 'compassionate',
            trustDelta: 7,
            respectDelta: 8,
            memoryText: 'Humbly volunteered to reinforce the Vanguard Bastion.'
          },
          {
            id: 'ter_c2',
            text: '“A blunt weapon can break armor, Teresa, but it cannot mend the firmament.”',
            tone: 'inquisitive',
            respectDelta: 5,
            conflictDelta: 3
          },
          {
            id: 'ter_c3',
            text: '“I see the fatigue behind your armor. Even the Iron Warden deserves peace.”',
            tone: 'compassionate',
            comfortDelta: 8,
            affectionDelta: 4
          }
        ]
      };
    } else if (isVaelen) {
      return {
        id: 'vae_start',
        speaker: character.name,
        emotion: 'introspective',
        text: `“Ah, the Echo stirs. Three centuries I have mapped the celestial fracture lines from this archive. The constellations are shifting toward the Great Eclipse. Tell me, do you sense the leyline frequencies?”`,
        choices: [
          {
            id: 'vae_c1',
            text: '“I feel their warmth vibrating through the stones beneath our feet.”',
            tone: 'mystical',
            trustDelta: 8,
            respectDelta: 8
          },
          {
            id: 'vae_c2',
            text: '“Can your Astrolabe forecast when the wards will shatter?”',
            tone: 'inquisitive',
            respectDelta: 6
          }
        ]
      };
    } else {
      return {
        id: 'syl_start',
        speaker: character.name,
        emotion: 'smiling',
        text: `“A mortal vibration ripples across the lily pond... We remember your kind before the stars tore open. Will you sing the hymn of dawn with us, sweet wanderer?”`,
        choices: [
          {
            id: 'syl_c1',
            text: '“Sing, and I will listen with all my soul.”',
            tone: 'mystical',
            affectionDelta: 8,
            comfortDelta: 8
          },
          {
            id: 'syl_c2',
            text: '“What lies hidden beneath the roots of the Sunken Grove?”',
            tone: 'inquisitive',
            trustDelta: 6
          }
        ]
      };
    }
  };

  const [currentNode, setCurrentNode] = useState<DialogueNode>(getInitialNode());

  // Typing animation for dialogue
  useEffect(() => {
    setDisplayedText('');
    let idx = 0;
    const fullText = currentNode.text;
    const timer = setInterval(() => {
      if (idx < fullText.length) {
        setDisplayedText(prev => prev + fullText.charAt(idx));
        if (idx % 3 === 0) sound.playDialogueBlip();
        idx++;
      } else {
        clearInterval(timer);
      }
    }, 22);

    return () => clearInterval(timer);
  }, [currentNode]);

  const handleChoice = (choice: DialogueChoice) => {
    sound.playClick();
    setHistoryLog(prev => [
      ...prev,
      { speaker: character.name, text: currentNode.text },
      { speaker: 'Echo', text: choice.text }
    ]);

    if (choice.triggerEventId && onStartCinematic) {
      onStartCinematic(choice.triggerEventId);
      return;
    }

    onChoiceSelect(choice);

    // Dynamic response progression
    if (choice.tone === 'romantic' || choice.tone === 'flirtatious') {
      setCurrentNode({
        id: 'next_romantic',
        speaker: character.name,
        emotion: 'flustered',
        text: `A faint rosy tint blossoms across ${character.name}’s cheeks. “You have a perilous way with words, Echo... but I would be lying if I said I wished for you to stop.”`,
        choices: [
          {
            id: 'end_tender',
            text: 'Gently take her hand and share the warmth of the moment.',
            tone: 'romantic',
            affectionDelta: 5,
            comfortDelta: 5
          },
          {
            id: 'end_graceful',
            text: 'Smile warmly and return to our duties in the Sanctuary.',
            tone: 'compassionate',
            comfortDelta: 4
          }
        ]
      });
    } else {
      // General progression
      setCurrentNode({
        id: 'next_general',
        speaker: character.name,
        emotion: 'smiling',
        text: `“I appreciate your candor. The bond we forge here will decide whether Edenfall is restored or consumed by eternal darkness.”`,
        choices: [
          {
            id: 'end_close',
            text: '“I will see you soon, ' + character.name + '.”',
            tone: 'compassionate'
          }
        ]
      });
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 md:p-6 bg-slate-950/85 backdrop-blur-md">
      <div className="relative w-full max-w-5xl h-[92vh] max-h-[820px] bg-slate-900/95 border border-slate-700/80 rounded-3xl shadow-2xl overflow-hidden flex flex-col">
        {/* Top Header bar with Character info & Independent metrics */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-800 bg-slate-950/60">
          <div className="flex items-center gap-3">
            <span className="text-2xl">{character.portrait}</span>
            <div>
              <div className="flex items-center gap-2">
                <h2 className="font-cinzel text-lg font-bold text-amber-200">{character.name}</h2>
                <span className="text-xs px-2 py-0.5 rounded-full bg-amber-500/10 text-amber-400 border border-amber-500/30">
                  {character.stage}
                </span>
                <span className="text-xs text-slate-400">Age: {character.age}</span>
              </div>
              <p className="text-xs text-slate-400">{character.title} • {character.location}</p>
            </div>
          </div>

          {/* Action buttons */}
          <div className="flex items-center gap-2">
            <button
              onClick={() => setShowGiftModal(true)}
              className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-pink-950/50 hover:bg-pink-900/60 text-pink-300 border border-pink-700/40 text-xs font-semibold transition"
            >
              <Gift size={14} /> Offer Gift
            </button>
            <button
              onClick={() => setShowHistory(!showHistory)}
              className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-200 border border-slate-600 text-xs transition"
            >
              <BookOpen size={14} /> Log
            </button>
            <button
              onClick={onClose}
              className="p-1.5 rounded-lg bg-slate-800 hover:bg-rose-900/60 text-slate-300 hover:text-rose-200 transition"
            >
              <X size={18} />
            </button>
          </div>
        </div>

        {/* Dynamic 6-Dimension Relationship Metrics Bar (per Directive 4.2) */}
        <div className="grid grid-cols-3 sm:grid-cols-6 gap-2 px-6 py-2.5 bg-slate-950/40 border-b border-slate-800 text-xs">
          <div className="flex flex-col">
            <div className="flex justify-between text-rose-300">
              <span className="flex items-center gap-1"><Heart size={11} /> Affection</span>
              <span className="font-mono font-semibold">{character.affection}/100</span>
            </div>
            <div className="w-full bg-slate-800 h-1.5 rounded-full mt-1 overflow-hidden">
              <div className="bg-gradient-to-r from-pink-500 to-rose-500 h-full rounded-full transition-all duration-500" style={{ width: `${Math.min(100, Math.max(0, character.affection))}%` }} />
            </div>
          </div>

          <div className="flex flex-col">
            <div className="flex justify-between text-sky-300">
              <span className="flex items-center gap-1"><Shield size={11} /> Trust</span>
              <span className="font-mono font-semibold">{character.trust}/100</span>
            </div>
            <div className="w-full bg-slate-800 h-1.5 rounded-full mt-1 overflow-hidden">
              <div className="bg-sky-500 h-full rounded-full transition-all duration-500" style={{ width: `${Math.min(100, Math.max(0, character.trust))}%` }} />
            </div>
          </div>

          <div className="flex flex-col">
            <div className="flex justify-between text-amber-300">
              <span className="flex items-center gap-1"><Sparkles size={11} /> Familiarity</span>
              <span className="font-mono font-semibold">{character.familiarity}/100</span>
            </div>
            <div className="w-full bg-slate-800 h-1.5 rounded-full mt-1 overflow-hidden">
              <div className="bg-amber-500 h-full rounded-full transition-all duration-500" style={{ width: `${Math.min(100, Math.max(0, character.familiarity))}%` }} />
            </div>
          </div>

          <div className="flex flex-col">
            <div className="flex justify-between text-emerald-300">
              <span>Respect</span>
              <span className="font-mono font-semibold">{character.respect}/100</span>
            </div>
            <div className="w-full bg-slate-800 h-1.5 rounded-full mt-1 overflow-hidden">
              <div className="bg-emerald-500 h-full rounded-full transition-all duration-500" style={{ width: `${Math.min(100, Math.max(0, character.respect))}%` }} />
            </div>
          </div>

          <div className="flex flex-col">
            <div className="flex justify-between text-purple-300">
              <span>Comfort</span>
              <span className="font-mono font-semibold">{character.comfort}/100</span>
            </div>
            <div className="w-full bg-slate-800 h-1.5 rounded-full mt-1 overflow-hidden">
              <div className="bg-purple-500 h-full rounded-full transition-all duration-500" style={{ width: `${Math.min(100, Math.max(0, character.comfort))}%` }} />
            </div>
          </div>

          <div className="flex flex-col">
            <div className="flex justify-between text-orange-400">
              <span>Conflict</span>
              <span className="font-mono font-semibold">{character.conflict}/100</span>
            </div>
            <div className="w-full bg-slate-800 h-1.5 rounded-full mt-1 overflow-hidden">
              <div className="bg-orange-500 h-full rounded-full transition-all duration-500" style={{ width: `${Math.min(100, Math.max(0, character.conflict))}%` }} />
            </div>
          </div>
        </div>

        {/* Main Stage: Anime Portrait & Dialogue Box */}
        <div className="relative flex-1 flex flex-col md:flex-row items-center justify-center p-4 md:p-8 overflow-hidden gap-6">
          {/* Animated Anime Vector Portrait */}
          <div className="flex-shrink-0 flex items-center justify-center">
            <AnimePortrait
              characterId={character.id}
              emotion={currentNode.emotion || 'gentle'}
              visionLayer={visionLayer}
              size="xl"
            />
          </div>

          {/* Dialogue & Choices Column */}
          <div className="flex-1 w-full max-w-2xl flex flex-col justify-end space-y-4">
            {/* Dialogue Bubble */}
            <div className="bg-slate-950/85 border border-slate-700/80 p-5 rounded-2xl shadow-xl relative min-h-[140px] flex flex-col justify-between">
              <div>
                <div className="flex items-center justify-between mb-2">
                  <span className="text-xs font-cinzel font-bold text-amber-300 uppercase tracking-widest">
                    {character.name}
                  </span>
                  <span className="text-xs text-slate-400 italic">
                    [{currentNode.emotion}]
                  </span>
                </div>
                <p className="text-slate-100 text-sm md:text-base leading-relaxed font-sans font-normal">
                  {displayedText}
                </p>
              </div>

              {/* Memory acknowledgment if present */}
              {character.memories.length > 0 && (
                <div className="mt-3 pt-2 border-t border-slate-800/80 text-[11px] text-amber-300/80 italic flex items-center gap-1.5">
                  <Sparkles size={11} /> {character.memories[character.memories.length - 1].text}
                </div>
              )}
            </div>

            {/* Interactive Dialogue Choices */}
            <div className="space-y-2">
              {currentNode.choices.map((choice, idx) => {
                const isRomance = choice.tone === 'romantic' || choice.tone === 'flirtatious';
                const isDefiant = choice.tone === 'defiant';

                return (
                  <button
                    key={choice.id || idx}
                    onClick={() => handleChoice(choice)}
                    className={`w-full text-left p-3.5 rounded-xl border text-xs md:text-sm transition-all duration-200 transform hover:-translate-y-0.5 shadow-md flex items-center justify-between ${
                      isRomance
                        ? 'bg-rose-950/40 hover:bg-rose-900/60 border-rose-500/50 text-rose-100 hover:border-rose-400'
                        : isDefiant
                        ? 'bg-slate-900/80 hover:bg-slate-800 border-amber-600/40 text-slate-200 hover:border-amber-400'
                        : 'bg-slate-900/80 hover:bg-slate-800 border-slate-700 text-slate-200 hover:border-sky-400'
                    }`}
                  >
                    <div className="flex items-center gap-2.5">
                      <span className="font-mono text-xs text-slate-400">{idx + 1}.</span>
                      <span className="font-medium">{choice.text}</span>
                    </div>

                    {choice.tone && (
                      <span className={`text-[10px] uppercase font-semibold px-2 py-0.5 rounded-full ${
                        isRomance ? 'bg-rose-500/20 text-rose-300 border border-rose-500/30' : 'bg-slate-800 text-slate-400'
                      }`}>
                        {choice.tone}
                      </span>
                    )}
                  </button>
                );
              })}
            </div>
          </div>
        </div>

        {/* Gift Offering Modal */}
        {showGiftModal && (
          <div className="absolute inset-0 z-20 bg-slate-950/90 backdrop-blur-md flex items-center justify-center p-6">
            <div className="bg-slate-900 border border-pink-600/50 rounded-2xl p-6 max-w-md w-full shadow-2xl">
              <div className="flex items-center justify-between mb-4">
                <h3 className="font-cinzel text-lg font-bold text-pink-300 flex items-center gap-2">
                  <Gift size={18} /> Offer Gift to {character.name}
                </h3>
                <button onClick={() => setShowGiftModal(false)} className="text-slate-400 hover:text-white">
                  <X size={18} />
                </button>
              </div>

              <p className="text-xs text-slate-300 mb-4">
                Selecting a gift that resonates with {character.name}’s affinities deepens affection and trust.
              </p>

              <div className="space-y-2 max-h-60 overflow-y-auto mb-4">
                {inventory.length === 0 ? (
                  <p className="text-xs text-slate-500 italic py-4 text-center">Your satchel is empty.</p>
                ) : (
                  inventory.map(item => (
                    <button
                      key={item.id}
                      onClick={() => {
                        onGiftItem(character.id, item);
                        setShowGiftModal(false);
                        sound.playQuestFanfare();
                      }}
                      className="w-full flex items-center justify-between p-2.5 rounded-xl bg-slate-800/80 hover:bg-pink-950/40 border border-slate-700 hover:border-pink-500/50 transition text-left"
                    >
                      <div className="flex items-center gap-2.5">
                        <span className="text-xl">{item.icon}</span>
                        <div>
                          <div className="text-xs font-semibold text-slate-100">{item.name}</div>
                          <div className="text-[10px] text-slate-400">{item.description}</div>
                        </div>
                      </div>
                      <span className="text-xs text-pink-400 font-bold">x{item.quantity}</span>
                    </button>
                  ))
                )}
              </div>

              <button
                onClick={() => setShowGiftModal(false)}
                className="w-full py-2 rounded-xl bg-slate-800 text-slate-300 text-xs font-semibold hover:bg-slate-700 transition"
              >
                Cancel
              </button>
            </div>
          </div>
        )}

        {/* Dialogue History Modal */}
        {showHistory && (
          <div className="absolute inset-0 z-20 bg-slate-950/90 backdrop-blur-md flex items-center justify-center p-6">
            <div className="bg-slate-900 border border-slate-700 rounded-2xl p-6 max-w-xl w-full h-[70vh] flex flex-col shadow-2xl">
              <div className="flex items-center justify-between mb-4">
                <h3 className="font-cinzel text-lg font-bold text-amber-300 flex items-center gap-2">
                  <BookOpen size={18} /> Conversation History
                </h3>
                <button onClick={() => setShowHistory(false)} className="text-slate-400 hover:text-white">
                  <X size={18} />
                </button>
              </div>
              <div className="flex-1 overflow-y-auto space-y-3 pr-2">
                {historyLog.map((log, i) => (
                  <div key={i} className="text-xs">
                    <span className="font-bold text-amber-400">{log.speaker}: </span>
                    <span className="text-slate-200">{log.text}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
