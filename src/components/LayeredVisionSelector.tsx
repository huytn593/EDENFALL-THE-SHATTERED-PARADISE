import React from 'react';
import { VisionLayer } from '../types';
import { Eye, Sparkles, Zap, Flame, ShieldAlert } from 'lucide-react';
import { sound } from '../utils/audio';

interface LayeredVisionSelectorProps {
  currentLayer: VisionLayer;
  onChangeLayer: (layer: VisionLayer) => void;
}

export const LayeredVisionSelector: React.FC<LayeredVisionSelectorProps> = ({
  currentLayer,
  onChangeLayer
}) => {
  const layers: { id: VisionLayer; label: string; icon: React.ReactNode; color: string; desc: string }[] = [
    {
      id: 'normal',
      label: 'Normal Sight',
      icon: <Eye size={14} />,
      color: 'border-slate-600 text-slate-300 hover:border-slate-400',
      desc: 'Natural vision of Edenfall'
    },
    {
      id: 'magical_aura',
      label: 'Magical Aura',
      icon: <Sparkles size={14} />,
      color: 'border-amber-500/80 text-amber-300 bg-amber-500/10',
      desc: 'Reveals ancient celestial runes'
    },
    {
      id: 'energy_pathways',
      label: 'Leyline Conduits',
      icon: <Zap size={14} />,
      color: 'border-cyan-500/80 text-cyan-300 bg-cyan-500/10',
      desc: 'Visualizes flow of aether streams'
    },
    {
      id: 'celestial_silhouette',
      label: 'Aura Silhouette',
      icon: <Flame size={14} />,
      color: 'border-purple-500/80 text-purple-300 bg-purple-500/10',
      desc: 'Perceives soul frequencies & bonds'
    },
    {
      id: 'miasma_sight',
      label: 'Miasma Warning',
      icon: <ShieldAlert size={14} />,
      color: 'border-rose-500/80 text-rose-300 bg-rose-500/10',
      desc: 'Highlights abyssal corruption hazards'
    }
  ];

  return (
    <div className="flex flex-wrap items-center gap-2 p-2 bg-slate-900/90 backdrop-blur-md rounded-2xl border border-slate-800 shadow-lg">
      <span className="text-[11px] font-cinzel font-bold text-slate-400 uppercase tracking-wider px-2">
        Layered Vision:
      </span>
      {layers.map(l => {
        const isActive = currentLayer === l.id;
        return (
          <button
            key={l.id}
            onClick={() => {
              onChangeLayer(l.id);
              sound.playClick();
            }}
            title={l.desc}
            className={`flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-medium transition-all ${
              isActive
                ? `${l.color} shadow-md scale-105 border`
                : 'text-slate-400 hover:text-slate-200 border border-transparent hover:bg-slate-800/60'
            }`}
          >
            {l.icon}
            <span>{l.label}</span>
          </button>
        );
      })}
    </div>
  );
};
