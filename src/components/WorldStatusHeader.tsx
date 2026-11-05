import React from 'react';
import { GameState, TimeOfDay } from '../types';
import { Sun, Moon, Sunrise, Sunset, Wind, Sparkles, Heart, Package, Compass, Save, Volume2, VolumeX, Image as ImageIcon } from 'lucide-react';
import { sound } from '../utils/audio';

interface WorldStatusHeaderProps {
  gameState: GameState;
  activeTab: 'explore' | 'relationships' | 'inventory' | 'quests' | 'gallery';
  isMuted: boolean;
  onTabChange: (tab: 'explore' | 'relationships' | 'inventory' | 'quests' | 'gallery') => void;
  onAdvanceTime: () => void;
  onToggleSound: () => void;
  onOpenSave: () => void;
}

export const WorldStatusHeader: React.FC<WorldStatusHeaderProps> = ({
  gameState,
  activeTab,
  isMuted,
  onTabChange,
  onAdvanceTime,
  onToggleSound,
  onOpenSave
}) => {
  const time = gameState.world.timeOfDay;

  const timeIcons: Record<TimeOfDay, React.ReactNode> = {
    Morning: <Sunrise className="text-amber-300" size={16} />,
    Afternoon: <Sun className="text-yellow-400" size={16} />,
    Evening: <Sunset className="text-rose-400" size={16} />,
    Night: <Moon className="text-sky-300" size={16} />
  };

  return (
    <header className="w-full bg-slate-950/90 border-b border-slate-800/80 backdrop-blur-md px-4 py-3 sticky top-0 z-40">
      <div className="max-w-7xl mx-auto flex flex-col md:flex-row items-center justify-between gap-3">
        {/* Brand & World State */}
        <div className="flex items-center gap-4 w-full md:w-auto justify-between md:justify-start">
          <div className="flex items-center gap-2.5">
            <span className="text-2xl animate-float">🌌</span>
            <div>
              <h1 className="font-cinzel text-sm sm:text-base font-black tracking-widest text-transparent bg-clip-text bg-gradient-to-r from-amber-200 via-amber-400 to-yellow-100 uppercase">
                Edenfall
              </h1>
              <p className="text-[10px] text-slate-400 tracking-wider">The Shattered Paradise</p>
            </div>
          </div>

          <div className="h-6 w-px bg-slate-800 hidden sm:block" />

          {/* Time & Weather Simulation */}
          <div className="flex items-center gap-2">
            <div className="flex items-center gap-1.5 px-3 py-1 rounded-xl bg-slate-900 border border-slate-800 text-xs">
              {timeIcons[time]}
              <span className="font-semibold text-slate-200">Day {gameState.world.dayNumber}</span>
              <span className="text-slate-500">•</span>
              <span className="text-amber-300 font-medium">{time}</span>
            </div>

            <button
              onClick={() => {
                onAdvanceTime();
                sound.playCrystalChime();
              }}
              title="Rest and advance to the next time of day"
              className="px-2.5 py-1 rounded-xl bg-slate-900 hover:bg-slate-800 border border-slate-800 hover:border-amber-500/40 text-[11px] font-semibold text-slate-300 transition flex items-center gap-1"
            >
              Advance Time
            </button>
          </div>
        </div>

        {/* Player Status Gauges (Stamina, Luminary Resonance, Restoration) */}
        <div className="flex items-center gap-4 text-xs w-full md:w-auto justify-center">
          {/* Stamina */}
          <div className="flex items-center gap-2">
            <span className="text-slate-400 font-medium text-[11px]">Stamina:</span>
            <div className="w-20 bg-slate-900 h-2 rounded-full border border-slate-800 overflow-hidden">
              <div
                className="bg-emerald-400 h-full rounded-full transition-all duration-300"
                style={{ width: `${Math.max(0, Math.min(100, gameState.player.stamina))}%` }}
              />
            </div>
          </div>

          {/* Luminary Resonance */}
          <div className="flex items-center gap-2">
            <span className="text-amber-300 font-medium text-[11px]">Resonance:</span>
            <div className="w-20 bg-slate-900 h-2 rounded-full border border-slate-800 overflow-hidden">
              <div
                className="bg-amber-400 h-full rounded-full transition-all duration-300"
                style={{ width: `${Math.max(0, Math.min(100, gameState.player.luminaryResonance))}%` }}
              />
            </div>
          </div>

          {/* Sanctuary Restoration */}
          <div className="flex items-center gap-2">
            <span className="text-sky-300 font-medium text-[11px]">Wards:</span>
            <div className="w-20 bg-slate-900 h-2 rounded-full border border-slate-800 overflow-hidden">
              <div
                className="bg-sky-400 h-full rounded-full transition-all duration-300"
                style={{ width: `${Math.max(0, Math.min(100, gameState.world.sanctuaryRestoration))}%` }}
              />
            </div>
          </div>
        </div>

        {/* Navigation Tabs & Utilities */}
        <div className="flex items-center gap-1.5 w-full md:w-auto justify-center md:justify-end overflow-x-auto">
          <button
            onClick={() => {
              onTabChange('explore');
              sound.playClick();
            }}
            className={`flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold transition ${
              activeTab === 'explore'
                ? 'bg-amber-500 text-slate-950 font-bold shadow'
                : 'text-slate-300 hover:text-white hover:bg-slate-900'
            }`}
          >
            <Sparkles size={14} /> Explore
          </button>

          <button
            onClick={() => {
              onTabChange('relationships');
              sound.playClick();
            }}
            className={`flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold transition ${
              activeTab === 'relationships'
                ? 'bg-amber-500 text-slate-950 font-bold shadow'
                : 'text-slate-300 hover:text-white hover:bg-slate-900'
            }`}
          >
            <Heart size={14} /> Bonds
          </button>

          <button
            onClick={() => {
              onTabChange('quests');
              sound.playClick();
            }}
            className={`flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold transition ${
              activeTab === 'quests'
                ? 'bg-amber-500 text-slate-950 font-bold shadow'
                : 'text-slate-300 hover:text-white hover:bg-slate-900'
            }`}
          >
            <Compass size={14} /> Quests
          </button>

          <button
            onClick={() => {
              onTabChange('inventory');
              sound.playClick();
            }}
            className={`flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold transition ${
              activeTab === 'inventory'
                ? 'bg-amber-500 text-slate-950 font-bold shadow'
                : 'text-slate-300 hover:text-white hover:bg-slate-900'
            }`}
          >
            <Package size={14} /> Satchel
          </button>

          <button
            onClick={() => {
              onTabChange('gallery');
              sound.playClick();
            }}
            className={`flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold transition ${
              activeTab === 'gallery'
                ? 'bg-amber-500 text-slate-950 font-bold shadow'
                : 'text-slate-300 hover:text-white hover:bg-slate-900'
            }`}
          >
            <ImageIcon size={14} /> CGs
          </button>

          <div className="h-4 w-px bg-slate-800 mx-1" />

          {/* Save/Load Button */}
          <button
            onClick={() => {
              onOpenSave();
              sound.playClick();
            }}
            title="Save / Load Chronicles"
            className="p-1.5 rounded-xl bg-slate-900 hover:bg-slate-800 text-slate-300 hover:text-white border border-slate-800 transition"
          >
            <Save size={15} />
          </button>

          {/* Sound Mute Toggle */}
          <button
            onClick={onToggleSound}
            title={isMuted ? 'Unmute Audio' : 'Mute Audio'}
            className="p-1.5 rounded-xl bg-slate-900 hover:bg-slate-800 text-slate-300 hover:text-white border border-slate-800 transition"
          >
            {isMuted ? <VolumeX size={15} /> : <Volume2 size={15} />}
          </button>
        </div>
      </div>
    </header>
  );
};
