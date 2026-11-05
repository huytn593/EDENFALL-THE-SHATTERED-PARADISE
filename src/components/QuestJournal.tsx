import React, { useState } from 'react';
import { Quest } from '../types';
import { Compass, CheckCircle2, Circle, Gift, X } from 'lucide-react';
import { sound } from '../utils/audio';

interface QuestJournalProps {
  quests: Quest[];
  onClose: () => void;
}

export const QuestJournal: React.FC<QuestJournalProps> = ({
  quests,
  onClose
}) => {
  const [selectedQuestId, setSelectedQuestId] = useState<string>(quests[0]?.id || '');
  const selectedQuest = quests.find(q => q.id === selectedQuestId) || quests[0];

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 md:p-6 bg-slate-950/85 backdrop-blur-md">
      <div className="relative w-full max-w-4xl h-[85vh] bg-slate-900/95 border border-slate-700/80 rounded-3xl shadow-2xl overflow-hidden flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-800 bg-slate-950/70">
          <div className="flex items-center gap-3">
            <Compass className="text-amber-400" size={22} />
            <div>
              <h2 className="font-cinzel text-lg font-bold text-amber-200">Chronicles of Edenfall</h2>
              <p className="text-xs text-slate-400">Sanctuary restoration goals, companion trials, and main saga</p>
            </div>
          </div>
          <button onClick={onClose} className="p-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-300">
            <X size={18} />
          </button>
        </div>

        {/* Content Layout */}
        <div className="flex-1 flex flex-col md:flex-row overflow-hidden">
          {/* Left Quests List */}
          <div className="w-full md:w-80 bg-slate-950/40 border-r border-slate-800 p-4 space-y-2 overflow-y-auto">
            {quests.map(quest => {
              const isSelected = quest.id === selectedQuestId;
              const isCompleted = quest.status === 'completed';

              return (
                <button
                  key={quest.id}
                  onClick={() => {
                    setSelectedQuestId(quest.id);
                    sound.playClick();
                  }}
                  className={`w-full p-3.5 rounded-2xl border text-left transition-all ${
                    isSelected
                      ? 'bg-amber-950/30 border-amber-500/50 shadow-md'
                      : 'bg-slate-900/60 border-slate-800 hover:bg-slate-800/60'
                  }`}
                >
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-[10px] uppercase font-bold px-2 py-0.5 rounded-full bg-slate-800 text-amber-300 border border-slate-700 font-mono">
                      {quest.category}
                    </span>
                    <span className={`text-xs font-semibold ${isCompleted ? 'text-emerald-400' : 'text-amber-400'}`}>
                      {quest.status}
                    </span>
                  </div>
                  <h3 className="font-cinzel text-xs font-bold text-slate-100 truncate">{quest.title}</h3>
                </button>
              );
            })}
          </div>

          {/* Right Quest Details */}
          {selectedQuest && (
            <div className="flex-1 p-6 overflow-y-auto space-y-6">
              <div>
                <div className="flex items-center gap-2 mb-1">
                  <span className="text-xs uppercase font-bold px-2.5 py-0.5 rounded-full bg-amber-500/15 text-amber-400 border border-amber-500/30 font-mono">
                    {selectedQuest.category}
                  </span>
                  <span className="text-xs text-slate-400">• Status: {selectedQuest.status}</span>
                </div>
                <h3 className="font-cinzel text-xl font-bold text-amber-100">{selectedQuest.title}</h3>
                <p className="text-xs text-slate-300 mt-2 leading-relaxed">{selectedQuest.summary}</p>
              </div>

              {/* Steps Checklist */}
              <div>
                <h4 className="font-cinzel text-xs font-bold text-slate-300 uppercase tracking-wider mb-3">
                  Current Objectives
                </h4>
                <div className="space-y-2.5">
                  {selectedQuest.steps.map((step, idx) => (
                    <div
                      key={idx}
                      className={`p-3 rounded-xl border flex items-center gap-3 text-xs ${
                        step.completed
                          ? 'bg-emerald-950/20 border-emerald-500/40 text-emerald-200'
                          : 'bg-slate-950/50 border-slate-800 text-slate-300'
                      }`}
                    >
                      {step.completed ? (
                        <CheckCircle2 size={16} className="text-emerald-400 flex-shrink-0" />
                      ) : (
                        <Circle size={16} className="text-slate-500 flex-shrink-0" />
                      )}
                      <span className={step.completed ? 'line-through opacity-80' : 'font-medium'}>
                        {step.description}
                      </span>
                    </div>
                  ))}
                </div>
              </div>

              {/* Rewards */}
              <div className="bg-slate-950/60 p-4 rounded-2xl border border-slate-800/80">
                <h4 className="text-xs font-bold text-amber-300 uppercase tracking-wider mb-1.5 flex items-center gap-1.5">
                  <Gift size={14} /> Rewards on Completion
                </h4>
                <p className="text-xs text-slate-300">{selectedQuest.rewardText}</p>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
