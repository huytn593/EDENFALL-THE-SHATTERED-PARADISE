import React, { useState } from 'react';
import { GameState } from '../types';
import { Save, Download, Upload, Clock, Check, X } from 'lucide-react';
import { sound } from '../utils/audio';

interface SaveLoadModalProps {
  currentGameState: GameState;
  onLoadGameState: (state: GameState) => void;
  onClose: () => void;
}

export const SaveLoadModal: React.FC<SaveLoadModalProps> = ({
  currentGameState,
  onLoadGameState,
  onClose
}) => {
  const [activeTab, setActiveTab] = useState<'save' | 'load'>('save');
  const [saveSlots, setSaveSlots] = useState<{ [key: string]: { date: string; time: string; state: GameState } }>(() => {
    try {
      const stored = localStorage.getItem('edenfall_save_slots');
      return stored ? JSON.parse(stored) : {};
    } catch {
      return {};
    }
  });

  const [notification, setNotification] = useState<string | null>(null);

  const handleSaveToSlot = (slotKey: string) => {
    sound.playQuestFanfare();
    const updated = {
      ...saveSlots,
      [slotKey]: {
        date: new Date().toLocaleDateString(),
        time: new Date().toLocaleTimeString(),
        state: currentGameState
      }
    };
    setSaveSlots(updated);
    localStorage.setItem('edenfall_save_slots', JSON.stringify(updated));
    setNotification(`Successfully saved to ${slotKey}!`);
    setTimeout(() => setNotification(null), 3000);
  };

  const handleLoadFromSlot = (slotKey: string) => {
    const slot = saveSlots[slotKey];
    if (slot) {
      sound.playQuestFanfare();
      onLoadGameState(slot.state);
      setNotification(`Loaded game state from ${slotKey}!`);
      setTimeout(() => {
        setNotification(null);
        onClose();
      }, 800);
    }
  };

  const handleExportJSON = () => {
    sound.playClick();
    const blob = new Blob([JSON.stringify(currentGameState, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `edenfall_save_${Date.now()}.json`;
    a.click();
    URL.revokeObjectURL(url);
  };

  const handleImportJSON = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (event) => {
      try {
        const imported = JSON.parse(event.target?.result as string);
        if (imported.player && imported.world && imported.relationships) {
          onLoadGameState(imported);
          sound.playQuestFanfare();
          setNotification('Successfully imported save file!');
          setTimeout(() => {
            setNotification(null);
            onClose();
          }, 800);
        } else {
          alert('Invalid Edenfall save format.');
        }
      } catch {
        alert('Failed to parse JSON save file.');
      }
    };
    reader.readAsText(file);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 md:p-6 bg-slate-950/85 backdrop-blur-md">
      <div className="relative w-full max-w-2xl bg-slate-900/95 border border-slate-700/80 rounded-3xl shadow-2xl overflow-hidden flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-800 bg-slate-950/70">
          <div className="flex items-center gap-3">
            <Save className="text-amber-400" size={22} />
            <div>
              <h2 className="font-cinzel text-lg font-bold text-amber-200">Chronicle Archives (Save / Load)</h2>
              <p className="text-xs text-slate-400">Preserve your journey, relationships, and world resonance</p>
            </div>
          </div>
          <button onClick={onClose} className="p-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-300">
            <X size={18} />
          </button>
        </div>

        {/* Tab switch */}
        <div className="flex items-center px-6 pt-4 gap-4 border-b border-slate-800">
          <button
            onClick={() => setActiveTab('save')}
            className={`pb-3 font-cinzel text-xs font-bold transition border-b-2 ${
              activeTab === 'save' ? 'border-amber-400 text-amber-300' : 'border-transparent text-slate-400 hover:text-slate-200'
            }`}
          >
            Save Progress
          </button>
          <button
            onClick={() => setActiveTab('load')}
            className={`pb-3 font-cinzel text-xs font-bold transition border-b-2 ${
              activeTab === 'load' ? 'border-amber-400 text-amber-300' : 'border-transparent text-slate-400 hover:text-slate-200'
            }`}
          >
            Load Progress
          </button>
        </div>

        {/* Notification banner */}
        {notification && (
          <div className="mx-6 mt-4 p-2.5 rounded-xl bg-emerald-950/50 border border-emerald-500/50 text-emerald-300 text-xs flex items-center gap-2">
            <Check size={14} /> {notification}
          </div>
        )}

        {/* Slots */}
        <div className="p-6 space-y-3">
          {['Slot 1 - Dawn Sanctuary', 'Slot 2 - Celestial Spire', 'Slot 3 - Romantic Echo'].map(slotKey => {
            const data = saveSlots[slotKey];
            return (
              <div
                key={slotKey}
                className="p-4 rounded-2xl bg-slate-950/60 border border-slate-800 flex items-center justify-between"
              >
                <div>
                  <h4 className="font-cinzel text-xs font-bold text-slate-200">{slotKey}</h4>
                  {data ? (
                    <div className="text-[11px] text-slate-400 mt-1 flex items-center gap-2">
                      <Clock size={12} className="text-amber-400" />
                      <span>{data.date} at {data.time}</span>
                      <span>• {data.state.world.timeOfDay}</span>
                    </div>
                  ) : (
                    <div className="text-[11px] text-slate-500 italic mt-1">Empty chronicle slot</div>
                  )}
                </div>

                <div>
                  {activeTab === 'save' ? (
                    <button
                      onClick={() => handleSaveToSlot(slotKey)}
                      className="px-4 py-2 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs transition"
                    >
                      Overwrite
                    </button>
                  ) : (
                    <button
                      disabled={!data}
                      onClick={() => handleLoadFromSlot(slotKey)}
                      className={`px-4 py-2 rounded-xl font-bold text-xs transition ${
                        data
                          ? 'bg-sky-500 hover:bg-sky-400 text-slate-950'
                          : 'bg-slate-800 text-slate-600 cursor-not-allowed'
                      }`}
                    >
                      Load
                    </button>
                  )}
                </div>
              </div>
            );
          })}
        </div>

        {/* JSON Export & Import */}
        <div className="p-6 pt-0 border-t border-slate-800/80 mt-2 flex items-center justify-between">
          <button
            onClick={handleExportJSON}
            className="flex items-center gap-1.5 px-3.5 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-xs font-semibold text-slate-200 border border-slate-700 transition"
          >
            <Download size={14} /> Export JSON File
          </button>

          <label className="flex items-center gap-1.5 px-3.5 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-xs font-semibold text-slate-200 border border-slate-700 cursor-pointer transition">
            <Upload size={14} /> Import JSON File
            <input type="file" accept=".json" onChange={handleImportJSON} className="hidden" />
          </label>
        </div>
      </div>
    </div>
  );
};
