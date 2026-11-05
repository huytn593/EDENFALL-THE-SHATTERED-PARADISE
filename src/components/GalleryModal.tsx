import React from 'react';
import { CinematicScene } from '../types';
import { CINEMATIC_SCENES } from '../data/cinematics';
import { Sparkles, Play, Lock, Eye, X } from 'lucide-react';
import { sound } from '../utils/audio';

interface GalleryModalProps {
  unlockedCGs: string[];
  onPlayScene: (scene: CinematicScene) => void;
  onClose: () => void;
}

export const GalleryModal: React.FC<GalleryModalProps> = ({
  unlockedCGs,
  onPlayScene,
  onClose
}) => {
  const allScenes = Object.values(CINEMATIC_SCENES);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 md:p-6 bg-slate-950/85 backdrop-blur-md">
      <div className="relative w-full max-w-4xl h-[85vh] bg-slate-900/95 border border-slate-700/80 rounded-3xl shadow-2xl overflow-hidden flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-800 bg-slate-950/70">
          <div className="flex items-center gap-3">
            <Sparkles className="text-amber-400" size={22} />
            <div>
              <h2 className="font-cinzel text-lg font-bold text-amber-200">Cinematic Event Gallery</h2>
              <p className="text-xs text-slate-400">Replay unlocked memories and intimate narrative milestones</p>
            </div>
          </div>
          <button onClick={onClose} className="p-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-300">
            <X size={18} />
          </button>
        </div>

        {/* Gallery Cards Grid */}
        <div className="flex-1 p-6 overflow-y-auto grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {allScenes.map(scene => {
            const isUnlocked = unlockedCGs.includes(scene.id) || scene.unlocked;

            return (
              <div
                key={scene.id}
                className={`relative rounded-2xl border overflow-hidden flex flex-col justify-between transition-all ${
                  isUnlocked
                    ? 'bg-slate-950/70 border-amber-500/40 hover:border-amber-400 shadow-lg'
                    : 'bg-slate-950/30 border-slate-800 opacity-60'
                }`}
              >
                {/* Visual Thumbnail Area */}
                <div className="relative h-44 w-full bg-gradient-to-br from-indigo-950 to-slate-950 flex items-center justify-center overflow-hidden">
                  {isUnlocked ? (
                    <div className="flex flex-col items-center justify-center p-4 text-center">
                      <span className="text-4xl mb-2 animate-bounce">✨</span>
                      <span className="font-cinzel text-sm font-bold text-amber-200">{scene.title}</span>
                      <span className="text-[11px] text-slate-400 mt-1">{scene.characterName}</span>
                    </div>
                  ) : (
                    <div className="flex flex-col items-center justify-center text-slate-500">
                      <Lock size={32} className="mb-2" />
                      <span className="text-xs font-semibold">Locked Memory</span>
                    </div>
                  )}

                  {/* Character participant badge */}
                  <span className="absolute top-2.5 left-2.5 px-2 py-0.5 rounded-md bg-slate-900/80 text-[10px] text-amber-300 border border-slate-700 font-mono">
                    {scene.characterName}
                  </span>
                </div>

                {/* Card Information */}
                <div className="p-4 flex-1 flex flex-col justify-between">
                  <div>
                    <h3 className="font-cinzel text-xs font-bold text-slate-200 mb-1">
                      {scene.title}
                    </h3>
                    <p className="text-[11px] text-slate-400 leading-relaxed line-clamp-2">
                      {scene.description}
                    </p>
                  </div>

                  <div className="mt-4 pt-3 border-t border-slate-800/80 flex items-center justify-between">
                    {isUnlocked ? (
                      <button
                        onClick={() => {
                          sound.playClick();
                          onPlayScene(scene);
                          onClose();
                        }}
                        className="w-full flex items-center justify-center gap-1.5 py-2 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs transition shadow"
                      >
                        <Play size={14} /> Replay Scene
                      </button>
                    ) : (
                      <div className="text-[10px] text-slate-500 italic">
                        Requires: {scene.unlockConditions.minAffection ? `${scene.unlockConditions.minAffection} Affection` : 'Story Progress'}
                      </div>
                    )}
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};
