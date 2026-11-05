import React from 'react';
import { VisionLayer } from '../types';

interface AnimePortraitProps {
  characterId: string;
  emotion: string; // 'gentle' | 'smiling' | 'flustered' | 'serious' | 'introspective' | 'pensive' | 'radiant' | 'resolute'
  visionLayer?: VisionLayer;
  className?: string;
  size?: 'sm' | 'md' | 'lg' | 'xl';
}

export const AnimePortrait: React.FC<AnimePortraitProps> = ({
  characterId,
  emotion = 'gentle',
  visionLayer = 'normal',
  className = '',
  size = 'lg'
}) => {
  const isAurelia = characterId === 'aurelia';
  const isTeresa = characterId === 'teresa';
  const isVaelen = characterId === 'vaelen';
  const isSylvara = characterId === 'sylvara';

  // Sizing styles
  const sizeClasses = {
    sm: 'w-16 h-16',
    md: 'w-28 h-28',
    lg: 'w-48 h-56 sm:w-60 sm:h-72',
    xl: 'w-72 h-88 sm:w-80 sm:h-96 md:w-96 md:h-[420px]'
  }[size];

  // Vision styling filters
  let filterStyle = '';
  let overlayAura = null;

  if (visionLayer === 'magical_aura') {
    filterStyle = 'drop-shadow(0 0 16px rgba(234, 179, 8, 0.75)) brightness(1.15)';
    overlayAura = (
      <div className="absolute inset-0 pointer-events-none rounded-2xl bg-amber-400/10 mix-blend-screen animate-pulse border border-amber-400/40" />
    );
  } else if (visionLayer === 'energy_pathways') {
    filterStyle = 'drop-shadow(0 0 12px rgba(6, 182, 212, 0.85)) hue-rotate(180deg) contrast(1.2)';
    overlayAura = (
      <div className="absolute inset-0 pointer-events-none rounded-2xl bg-cyan-500/10 mix-blend-color-dodge animate-pulse border border-cyan-400/50" />
    );
  } else if (visionLayer === 'celestial_silhouette') {
    filterStyle = 'drop-shadow(0 0 20px rgba(168, 85, 247, 0.9)) saturate(1.8) brightness(1.2)';
    overlayAura = (
      <div className="absolute inset-0 pointer-events-none rounded-2xl bg-purple-500/15 mix-blend-overlay border border-purple-400/60" />
    );
  } else if (visionLayer === 'miasma_sight') {
    filterStyle = 'drop-shadow(0 0 14px rgba(244, 63, 94, 0.8)) hue-rotate(320deg) contrast(1.3)';
    overlayAura = (
      <div className="absolute inset-0 pointer-events-none rounded-2xl bg-rose-950/20 mix-blend-color-burn border border-rose-500/40" />
    );
  }

  // Expression features
  const hasBlush = emotion === 'flustered' || emotion === 'radiant' || emotion === 'warm';
  const isSmile = emotion === 'smiling' || emotion === 'radiant' || emotion === 'gentle';
  const isPensive = emotion === 'introspective' || emotion === 'pensive';
  const isSerious = emotion === 'serious' || emotion === 'resolute';

  return (
    <div className={`relative flex items-center justify-center overflow-hidden rounded-2xl select-none ${sizeClasses} ${className}`}>
      {/* Background celestial ambient halo */}
      <div className="absolute inset-0 bg-gradient-to-t from-slate-950 via-slate-900/60 to-transparent rounded-2xl" />

      {/* SVG Canvas for Character Anime Vector Portrait */}
      <svg
        viewBox="0 0 300 360"
        className="w-full h-full object-cover transition-all duration-500 transform hover:scale-[1.02]"
        style={{ filter: filterStyle }}
      >
        <defs>
          <linearGradient id="hairGold" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stopColor="#fef08a" />
            <stop offset="50%" stopColor="#facc15" />
            <stop offset="100%" stopColor="#ca8a04" />
          </linearGradient>
          <linearGradient id="hairSilver" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stopColor="#ffffff" />
            <stop offset="60%" stopColor="#cbd5e1" />
            <stop offset="100%" stopColor="#64748b" />
          </linearGradient>
          <linearGradient id="hairCrimson" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stopColor="#fda4af" />
            <stop offset="50%" stopColor="#e11d48" />
            <stop offset="100%" stopColor="#881337" />
          </linearGradient>
          <linearGradient id="hairEmerald" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stopColor="#86efac" />
            <stop offset="50%" stopColor="#10b981" />
            <stop offset="100%" stopColor="#065f46" />
          </linearGradient>
          <linearGradient id="skinToneAurelia" x1="0%" y1="0%" x2="0%" y2="100%">
            <stop offset="0%" stopColor="#fff1f2" />
            <stop offset="100%" stopColor="#ffe4e6" />
          </linearGradient>
          <linearGradient id="dressAurelia" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stopColor="#1e1b4b" />
            <stop offset="60%" stopColor="#312e81" />
            <stop offset="100%" stopColor="#4338ca" />
          </linearGradient>
          <linearGradient id="armorTeresa" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stopColor="#94a3b8" />
            <stop offset="60%" stopColor="#475569" />
            <stop offset="100%" stopColor="#1e293b" />
          </linearGradient>
          <radialGradient id="celestialHalo" cx="50%" cy="30%" r="50%">
            <stop offset="0%" stopColor="rgba(251, 191, 36, 0.4)" />
            <stop offset="70%" stopColor="rgba(251, 191, 36, 0.05)" />
            <stop offset="100%" stopColor="transparent" />
          </radialGradient>
        </defs>

        {/* Ambient Halo behind head */}
        <circle cx="150" cy="130" r="110" fill="url(#celestialHalo)" className="animate-pulse" />

        {/* 1. Aurelia (Main Heroine / Luminary Heir) */}
        {isAurelia && (
          <g className="transition-all duration-300">
            {/* Long flowing back hair */}
            <path
              d="M 60 160 C 40 240 70 340 90 360 L 210 360 C 230 340 260 240 240 160 Z"
              fill="url(#hairGold)"
              opacity="0.9"
            />

            {/* Shoulders & Royal Celestial Robes */}
            <path
              d="M 80 360 C 90 280 120 250 150 250 C 180 250 210 280 220 360 Z"
              fill="url(#dressAurelia)"
            />
            {/* Gold embroidery on dress */}
            <path
              d="M 120 280 L 150 330 L 180 280"
              stroke="#facc15"
              strokeWidth="3"
              fill="none"
              strokeLinecap="round"
            />
            {/* Luminary Solstice Pendant */}
            <circle cx="150" cy="275" r="9" fill="#fef08a" stroke="#ca8a04" strokeWidth="2" />
            <path d="M 150 260 L 150 290 M 135 275 L 165 275" stroke="#ca8a04" strokeWidth="1.5" />

            {/* Neck */}
            <path d="M 135 210 L 135 250 L 165 250 L 165 210 Z" fill="url(#skinToneAurelia)" />

            {/* Face oval */}
            <path
              d="M 95 130 C 95 200 130 225 150 225 C 170 225 205 200 205 130 C 205 70 95 70 95 130 Z"
              fill="url(#skinToneAurelia)"
            />

            {/* Ears */}
            <path d="M 90 140 C 85 145 85 160 95 165 Z" fill="#ffe4e6" />
            <path d="M 210 140 C 215 145 215 160 205 165 Z" fill="#ffe4e6" />

            {/* Anime Eyes */}
            {/* Left Eye */}
            <ellipse cx="125" cy="142" rx="14" ry="16" fill="#1e1b4b" />
            <ellipse cx="125" cy="144" rx="11" ry="13" fill="#818cf8" />
            <circle cx="121" cy="138" r="4" fill="#ffffff" />
            <circle cx="128" cy="147" r="2.5" fill="#c7d2fe" />
            {/* Eye lashes & lid */}
            <path d="M 110 134 Q 125 125 140 134" stroke="#0f172a" strokeWidth="3" fill="none" strokeLinecap="round" />
            {isSerious && <path d="M 112 126 L 138 132" stroke="#4338ca" strokeWidth="2.5" fill="none" />}
            {isPensive && <path d="M 112 131 Q 125 128 138 126" stroke="#4338ca" strokeWidth="2.5" fill="none" />}

            {/* Right Eye */}
            <ellipse cx="175" cy="142" rx="14" ry="16" fill="#1e1b4b" />
            <ellipse cx="175" cy="144" rx="11" ry="13" fill="#818cf8" />
            <circle cx="171" cy="138" r="4" fill="#ffffff" />
            <circle cx="178" cy="147" r="2.5" fill="#c7d2fe" />
            {/* Eye lashes */}
            <path d="M 160 134 Q 175 125 190 134" stroke="#0f172a" strokeWidth="3" fill="none" strokeLinecap="round" />
            {isSerious && <path d="M 162 132 L 188 126" stroke="#4338ca" strokeWidth="2.5" fill="none" />}
            {isPensive && <path d="M 162 126 Q 175 128 188 131" stroke="#4338ca" strokeWidth="2.5" fill="none" />}

            {/* Cute nose */}
            <path d="M 150 162 L 148 168" stroke="#f472b6" strokeWidth="1.5" strokeLinecap="round" />

            {/* Anime Mouth */}
            {isSmile ? (
              <path d="M 141 185 Q 150 196 159 185" stroke="#e11d48" strokeWidth="2.5" fill="#fecdd3" strokeLinecap="round" />
            ) : isPensive ? (
              <path d="M 143 186 Q 150 188 157 185" stroke="#e11d48" strokeWidth="2" fill="none" strokeLinecap="round" />
            ) : (
              <path d="M 144 186 Q 150 189 156 186" stroke="#e11d48" strokeWidth="2" fill="none" strokeLinecap="round" />
            )}

            {/* Emotional Blush */}
            {hasBlush && (
              <>
                <ellipse cx="114" cy="164" rx="12" ry="6" fill="#f43f5e" opacity="0.45" />
                <ellipse cx="186" cy="164" rx="12" ry="6" fill="#f43f5e" opacity="0.45" />
              </>
            )}

            {/* Golden Hair Front & Bangs */}
            <path
              d="M 90 120 C 100 60 200 60 210 120 C 205 100 170 95 150 115 C 130 95 95 100 90 120 Z"
              fill="url(#hairGold)"
            />
            {/* Bang strands framing face */}
            <path d="M 95 100 C 95 160 85 200 80 230 C 95 210 110 170 110 125 Z" fill="url(#hairGold)" />
            <path d="M 205 100 C 205 160 215 200 220 230 C 205 210 190 170 190 125 Z" fill="url(#hairGold)" />
            {/* Center soft hair bang */}
            <path d="M 145 95 Q 150 130 140 145 Q 155 130 155 95 Z" fill="#fef08a" />

            {/* Starlight tiara / hairpin */}
            <polygon points="150,70 155,85 170,85 158,95 162,110 150,100 138,110 142,95 130,85 145,85" fill="#facc15" stroke="#ca8a04" strokeWidth="1" />
          </g>
        )}

        {/* 2. Commander Teresa (Vanguard Iron Warden) */}
        {isTeresa && (
          <g>
            {/* Crimson hair back */}
            <path d="M 70 160 C 50 250 80 340 95 360 L 205 360 C 220 340 250 250 230 160 Z" fill="url(#hairCrimson)" />
            {/* Vanguard Starmetal Armor */}
            <path d="M 70 360 C 80 270 120 240 150 240 C 180 240 220 270 230 360 Z" fill="url(#armorTeresa)" />
            {/* Gorget & Pauldrons */}
            <path d="M 100 280 L 150 320 L 200 280" stroke="#f59e0b" strokeWidth="4" fill="none" />
            <circle cx="150" cy="280" r="10" fill="#991b1b" stroke="#f59e0b" strokeWidth="2" />
            {/* Face */}
            <path d="M 100 130 C 100 205 130 225 150 225 C 170 225 200 205 200 130 C 200 70 100 70 100 130 Z" fill="#ffe4e6" />
            {/* Fierce Amber Eyes */}
            <ellipse cx="128" cy="142" rx="13" ry="14" fill="#78350f" />
            <ellipse cx="128" cy="143" rx="10" ry="11" fill="#f59e0b" />
            <circle cx="124" cy="139" r="3.5" fill="#ffffff" />
            <path d="M 112 130 L 142 135" stroke="#1f2937" strokeWidth="3.5" strokeLinecap="round" />

            <ellipse cx="172" cy="142" rx="13" ry="14" fill="#78350f" />
            <ellipse cx="172" cy="143" rx="10" ry="11" fill="#f59e0b" />
            <circle cx="168" cy="139" r="3.5" fill="#ffffff" />
            <path d="M 158 135 L 188 130" stroke="#1f2937" strokeWidth="3.5" strokeLinecap="round" />

            {/* Nose & Firm Mouth */}
            <path d="M 150 162 L 148 168" stroke="#f43f5e" strokeWidth="1.5" />
            <path d="M 142 186 L 158 186" stroke="#991b1b" strokeWidth="2.5" strokeLinecap="round" />

            {/* Sharp Ponytail Front */}
            <path d="M 95 110 C 105 60 195 60 205 110 C 190 95 160 95 150 115 C 140 95 110 95 95 110 Z" fill="url(#hairCrimson)" />
            <path d="M 90 110 L 80 200 L 105 150 Z" fill="url(#hairCrimson)" />
            <path d="M 210 110 L 220 200 L 195 150 Z" fill="url(#hairCrimson)" />
          </g>
        )}

        {/* 3. Archivist Vaelen */}
        {isVaelen && (
          <g>
            {/* Silver Scholar Hair */}
            <path d="M 75 140 C 60 260 80 340 95 360 L 205 360 C 220 340 240 260 225 140 Z" fill="url(#hairSilver)" />
            {/* Celestial Robes */}
            <path d="M 80 360 C 90 270 120 240 150 240 C 180 240 210 270 220 360 Z" fill="#0f172a" />
            <path d="M 130 250 L 150 350 L 170 250" stroke="#38bdf8" strokeWidth="3" fill="none" />
            {/* Face */}
            <path d="M 105 135 C 105 205 130 225 150 225 C 170 225 195 205 195 135 C 195 80 105 80 105 135 Z" fill="#f8fafc" />
            {/* Astrolabe Spectacles */}
            <circle cx="130" cy="144" r="14" fill="none" stroke="#38bdf8" strokeWidth="2.5" />
            <circle cx="170" cy="144" r="14" fill="none" stroke="#38bdf8" strokeWidth="2.5" />
            <line x1="144" y1="144" x2="156" y2="144" stroke="#38bdf8" strokeWidth="2.5" />
            <ellipse cx="130" cy="144" rx="8" ry="9" fill="#0284c7" />
            <ellipse cx="170" cy="144" rx="8" ry="9" fill="#0284c7" />
            {/* Wisdom Smile */}
            <path d="M 142 186 Q 150 192 158 186" stroke="#475569" strokeWidth="2" fill="none" />
            {/* Hair bangs */}
            <path d="M 100 115 C 110 75 190 75 200 115 C 180 105 160 105 150 120 C 140 105 120 105 100 115 Z" fill="url(#hairSilver)" />
          </g>
        )}

        {/* 4. Sylvara / The Resonant Echo (Emerald Forest Spirit) */}
        {(!isAurelia && !isTeresa && !isVaelen) && (
          <g>
            {/* Ethereal Forest Hair */}
            <path d="M 65 150 C 45 260 70 350 95 360 L 205 360 C 230 350 255 260 235 150 Z" fill="url(#hairEmerald)" />
            {/* Floral Bodice */}
            <path d="M 85 360 C 95 270 120 250 150 250 C 180 250 205 270 215 360 Z" fill="#064e3b" />
            <path d="M 125 270 Q 150 310 175 270" stroke="#6ee7b7" strokeWidth="3" fill="none" />
            {/* Face */}
            <path d="M 100 135 C 100 205 130 225 150 225 C 170 225 200 205 200 135 C 200 75 100 75 100 135 Z" fill="#ecfdf5" />
            {/* Emerald Eyes */}
            <ellipse cx="127" cy="144" rx="13" ry="15" fill="#047857" />
            <ellipse cx="127" cy="145" rx="10" ry="12" fill="#34d399" />
            <circle cx="123" cy="140" r="4" fill="#ffffff" />
            <ellipse cx="173" cy="144" rx="13" ry="15" fill="#047857" />
            <ellipse cx="173" cy="145" rx="10" ry="12" fill="#34d399" />
            <circle cx="169" cy="140" r="4" fill="#ffffff" />
            {/* Mysterious Smile */}
            <path d="M 142 186 Q 150 193 158 186" stroke="#059669" strokeWidth="2.5" fill="none" />
            {/* Floral Crown */}
            <circle cx="130" cy="90" r="8" fill="#34d399" />
            <circle cx="150" cy="85" r="9" fill="#a7f3d0" />
            <circle cx="170" cy="90" r="8" fill="#34d399" />
          </g>
        )}
      </svg>

      {/* Layered Vision Overlay */}
      {overlayAura}
    </div>
  );
};
