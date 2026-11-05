import React, { useState } from 'react';
import { CharacterRelationship, VisionLayer } from '../types';
import { AnimePortrait } from './AnimePortrait';
import { Heart, Shield, Sparkles, Clock, MapPin, Award, BookOpen, AlertCircle, X } from 'lucide-react';
import { sound } from '../utils/audio';

interface RelationshipJournalProps {
  relationships: Record<string, CharacterRelationship>;
  visionLayer: VisionLayer;
  onSelectCharacter: (char: CharacterRelationship) => void;
  onClose: () => void;
}

export const RelationshipJournal: React.FC<RelationshipJournalProps> = ({
  relationships,
  visionLayer,
  onSelectCharacter,
  onClose
}) => {
  const charList = Object.values(relationships);
  const [selectedCharId, setSelectedCharId] = useState(charList[0]?.id || 'aurelia');
  const selectedChar = relationships[selectedCharId] || charList[0];

  const times = ['Morning', 'Afternoon', 'Evening', 'Night'] as const;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 md:p-6 bg-slate-950/85 backdrop-blur-md">
      <div className="relative w-full max-w-5xl h-[88vh] bg-slate-900/95 border border-slate-700/80 rounded-3xl shadow-2xl overflow-hidden flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-800 bg-slate-950/70">
          <div className="flex items-center gap-3">
            <Heart className="text-rose-400" size={22} />
            <div>
              <h2 className="font-cinzel text-lg font-bold text-amber-200">Character & Relationship Journal</h2>
              <p className="text-xs text-slate-400">Detailed bonds, daily routines, memories, and affinities</p>
            </div>
          </div>
          <button onClick={onClose} className="p-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-300">
            <X size={18} />
          </button>
        </div>

        {/* Content Layout */}
        <div className="flex-1 flex flex-col md:flex-row overflow-hidden">
          {/* Left Character Selector Sidebar */}
          <div className="w-full md:w-72 bg-slate-950/40 border-r border-slate-800 p-3 space-y-2 overflow-y-auto">
            {charList.map(char => {
              const isSelected = char.id === selectedCharId;
              return (
                <button
                  key={char.id}
                  onClick={() => {
                    setSelectedCharId(char.id);
                    sound.playClick();
                  }}
                  className={`w-full flex items-center gap-3 p-3 rounded-2xl border text-left transition-all ${
                    isSelected
                      ? 'bg-amber-950/30 border-amber-500/50 text-amber-200 shadow-md'
                      : 'bg-slate-900/60 border-slate-800 text-slate-300 hover:bg-slate-800/60'
                  }`}
                >
                  <AnimePortrait characterId={char.id} emotion="gentle" size="sm" />
                  <div className="flex-1 min-w-0">
                    <div className="font-cinzel text-xs font-bold truncate">{char.name}</div>
                    <div className="text-[11px] text-slate-400 truncate">{char.role}</div>
                    <div className="text-[10px] text-rose-400 flex items-center gap-1 mt-0.5">
                      <Heart size={10} /> {char.affection}/100 • <span className="text-sky-300">{char.stage}</span>
                    </div>
                  </div>
                </button>
              );
            })}
          </div>

          {/* Right Main Character Profile */}
          {selectedChar && (
            <div className="flex-1 p-6 overflow-y-auto space-y-6">
              {/* Profile Top Summary */}
              <div className="flex flex-col sm:flex-row items-center gap-6 bg-slate-950/50 p-5 rounded-2xl border border-slate-800">
                <AnimePortrait
                  characterId={selectedChar.id}
                  emotion={selectedChar.currentEmotion}
                  visionLayer={visionLayer}
                  size="lg"
                />
                <div className="flex-1 space-y-2 text-center sm:text-left">
                  <div className="flex flex-wrap items-center justify-center sm:justify-start gap-2">
                    <h3 className="font-cinzel text-xl font-bold text-amber-100">{selectedChar.name}</h3>
                    <span className="px-2.5 py-0.5 rounded-full bg-rose-500/15 text-rose-300 border border-rose-500/30 text-xs font-semibold">
                      {selectedChar.stage}
                    </span>
                    <span className="text-xs text-slate-400">Age: {selectedChar.age}</span>
                  </div>
                  <p className="text-xs text-amber-300/90 font-medium">{selectedChar.title}</p>
                  <p className="text-xs text-slate-300 leading-relaxed">{selectedChar.bio}</p>

                  <div className="pt-2 flex flex-wrap gap-2 justify-center sm:justify-start">
                    <button
                      onClick={() => {
                        onSelectCharacter(selectedChar);
                        onClose();
                      }}
                      className="px-4 py-2 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs transition shadow"
                    >
                      Enter Dialogue Mode
                    </button>
                  </div>
                </div>
              </div>

              {/* 6 Independent Relationship Dimension Gauges */}
              <div>
                <h4 className="font-cinzel text-sm font-bold text-slate-200 mb-3 flex items-center gap-2">
                  <Sparkles size={16} className="text-amber-400" /> Relationship Dimensions
                </h4>
                <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                  <div className="bg-slate-950/40 p-3 rounded-xl border border-slate-800">
                    <div className="flex justify-between text-xs text-rose-300 mb-1">
                      <span className="flex items-center gap-1 font-semibold"><Heart size={12} /> Affection</span>
                      <span className="font-mono">{selectedChar.affection}/100</span>
                    </div>
                    <div className="w-full bg-slate-800 h-2 rounded-full overflow-hidden">
                      <div className="bg-rose-500 h-full rounded-full" style={{ width: `${selectedChar.affection}%` }} />
                    </div>
                  </div>

                  <div className="bg-slate-950/40 p-3 rounded-xl border border-slate-800">
                    <div className="flex justify-between text-xs text-sky-300 mb-1">
                      <span className="flex items-center gap-1 font-semibold"><Shield size={12} /> Trust</span>
                      <span className="font-mono">{selectedChar.trust}/100</span>
                    </div>
                    <div className="w-full bg-slate-800 h-2 rounded-full overflow-hidden">
                      <div className="bg-sky-500 h-full rounded-full" style={{ width: `${selectedChar.trust}%` }} />
                    </div>
                  </div>

                  <div className="bg-slate-950/40 p-3 rounded-xl border border-slate-800">
                    <div className="flex justify-between text-xs text-amber-300 mb-1">
                      <span className="font-semibold">Familiarity</span>
                      <span className="font-mono">{selectedChar.familiarity}/100</span>
                    </div>
                    <div className="w-full bg-slate-800 h-2 rounded-full overflow-hidden">
                      <div className="bg-amber-500 h-full rounded-full" style={{ width: `${selectedChar.familiarity}%` }} />
                    </div>
                  </div>

                  <div className="bg-slate-950/40 p-3 rounded-xl border border-slate-800">
                    <div className="flex justify-between text-xs text-emerald-300 mb-1">
                      <span className="font-semibold">Respect</span>
                      <span className="font-mono">{selectedChar.respect}/100</span>
                    </div>
                    <div className="w-full bg-slate-800 h-2 rounded-full overflow-hidden">
                      <div className="bg-emerald-500 h-full rounded-full" style={{ width: `${selectedChar.respect}%` }} />
                    </div>
                  </div>

                  <div className="bg-slate-950/40 p-3 rounded-xl border border-slate-800">
                    <div className="flex justify-between text-xs text-purple-300 mb-1">
                      <span className="font-semibold">Emotional Comfort</span>
                      <span className="font-mono">{selectedChar.comfort}/100</span>
                    </div>
                    <div className="w-full bg-slate-800 h-2 rounded-full overflow-hidden">
                      <div className="bg-purple-500 h-full rounded-full" style={{ width: `${selectedChar.comfort}%` }} />
                    </div>
                  </div>

                  <div className="bg-slate-950/40 p-3 rounded-xl border border-slate-800">
                    <div className="flex justify-between text-xs text-orange-400 mb-1">
                      <span className="font-semibold">Conflict</span>
                      <span className="font-mono">{selectedChar.conflict}/100</span>
                    </div>
                    <div className="w-full bg-slate-800 h-2 rounded-full overflow-hidden">
                      <div className="bg-orange-500 h-full rounded-full" style={{ width: `${selectedChar.conflict}%` }} />
                    </div>
                  </div>
                </div>
              </div>

              {/* Daily Schedule Simulation System (per Directive 4.1) */}
              <div>
                <h4 className="font-cinzel text-sm font-bold text-slate-200 mb-3 flex items-center gap-2">
                  <Clock size={16} className="text-amber-400" /> Daily Schedule & Routine
                </h4>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
                  {times.map(t => {
                    const sched = selectedChar.schedules[t];
                    return (
                      <div key={t} className="p-3 rounded-xl bg-slate-950/50 border border-slate-800 text-xs flex items-start gap-3">
                        <span className="px-2 py-1 rounded bg-slate-800 text-amber-300 font-semibold font-mono text-[10px] uppercase">
                          {t}
                        </span>
                        <div>
                          <div className="text-slate-200 font-medium">{sched.activity}</div>
                          <div className="text-[11px] text-slate-400 mt-0.5 flex items-center gap-1">
                            <MapPin size={10} /> Location: {sched.locationId.replace('loc_', '').toUpperCase()}
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>

              {/* Structured Memories Log (per Directive 4.3) */}
              <div>
                <h4 className="font-cinzel text-sm font-bold text-slate-200 mb-3 flex items-center gap-2">
                  <BookOpen size={16} className="text-amber-400" /> Structured Memory Records
                </h4>
                <div className="space-y-2">
                  {selectedChar.memories.map(mem => (
                    <div key={mem.id} className="p-3 rounded-xl bg-slate-950/60 border border-slate-800/80 text-xs flex items-center justify-between">
                      <div className="flex items-center gap-2.5">
                        <span className={`w-2 h-2 rounded-full ${
                          mem.impact === 'positive' || mem.impact === 'romantic' ? 'bg-emerald-400' : mem.impact === 'negative' ? 'bg-rose-400' : 'bg-slate-400'
                        }`} />
                        <span className="text-slate-200">{mem.text}</span>
                      </div>
                      <span className="text-[10px] text-slate-500 font-mono">{mem.timestamp}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
