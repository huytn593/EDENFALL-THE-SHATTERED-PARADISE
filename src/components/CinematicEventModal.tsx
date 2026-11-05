import React, { useState, useEffect } from 'react';
import { CinematicScene } from '../types';
import { AnimePortrait } from './AnimePortrait';
import { sound } from '../utils/audio';
import { Play, SkipForward, Sparkles, Heart, ChevronRight, X } from 'lucide-react';
import confetti from 'canvas-confetti';

interface CinematicEventModalProps {
  scene: CinematicScene;
  onComplete: (scene: CinematicScene) => void;
  onClose: () => void;
}

export const CinematicEventModal: React.FC<CinematicEventModalProps> = ({
  scene,
  onComplete,
  onClose
}) => {
  const [currentSlideIndex, setCurrentSlideIndex] = useState(0);
  const [fadeBlack, setFadeBlack] = useState(false);
  const [textVisible, setTextVisible] = useState(true);

  const currentSlide = scene.slides[currentSlideIndex];
  const isLastSlide = currentSlideIndex === scene.slides.length - 1;

  useEffect(() => {
    sound.playCrystalChime();
  }, [currentSlideIndex]);

  const handleNext = () => {
    sound.playClick();
    if (isLastSlide) {
      // Fade to black transition for romantic climax
      setFadeBlack(true);
      setTimeout(() => {
        try {
          confetti({
            particleCount: 60,
            spread: 70,
            origin: { y: 0.6 },
            colors: ['#f43f5e', '#fbbf24', '#e0e7ff']
          });
        } catch {
          // ignore
        }
        sound.playQuestFanfare();
        onComplete(scene);
      }, 900);
    } else {
      setTextVisible(false);
      setTimeout(() => {
        setCurrentSlideIndex(prev => prev + 1);
        setTextVisible(true);
      }, 300);
    }
  };

  const handleSkip = () => {
    sound.playClick();
    setFadeBlack(true);
    setTimeout(() => {
      onComplete(scene);
    }, 400);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black">
      {/* Cinematic Widescreen Letterbox Bars */}
      <div className="absolute top-0 left-0 right-0 h-10 md:h-16 bg-black z-30 flex items-center justify-between px-6">
        <div className="flex items-center gap-2">
          <span className="text-amber-400 font-cinzel text-xs md:text-sm tracking-widest uppercase">
            Edenfall Cinematic Memory
          </span>
          <span className="text-slate-500 text-xs">•</span>
          <span className="text-slate-300 text-xs font-semibold">{scene.title}</span>
        </div>
        <button
          onClick={handleSkip}
          className="flex items-center gap-1 px-3 py-1 rounded bg-slate-900/80 hover:bg-slate-800 text-slate-400 hover:text-white text-xs transition border border-slate-700"
        >
          <SkipForward size={13} /> Skip Scene
        </button>
      </div>

      <div className="absolute bottom-0 left-0 right-0 h-10 md:h-16 bg-black z-30 flex items-center justify-between px-6 text-xs text-slate-500">
        <div>Slide {currentSlideIndex + 1} of {scene.slides.length}</div>
        <div className="text-slate-400 italic">Click anywhere or press Advance to continue</div>
      </div>

      {/* Main Cinematic Visual Stage */}
      <div
        onClick={handleNext}
        className={`relative w-full h-full flex flex-col items-center justify-center p-6 md:p-12 overflow-hidden cursor-pointer transition-opacity duration-700 ${
          fadeBlack ? 'opacity-0 bg-black' : 'opacity-100'
        }`}
      >
        {/* Dynamic CG Celestial Visual Background */}
        <div className="absolute inset-0 bg-gradient-to-b from-slate-950 via-indigo-950/40 to-slate-950 z-0" />

        {/* Ambient Starlight particles and light beam effect */}
        <div className="absolute inset-0 pointer-events-none z-10 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-amber-400/10 via-transparent to-transparent" />

        {/* Character Portrait with Cinematic Framing */}
        <div className="relative z-20 flex flex-col items-center justify-center mb-8 transform transition-transform duration-700">
          <AnimePortrait
            characterId={scene.characterId}
            emotion={currentSlide.characterEmotion}
            size="xl"
            className="filter drop-shadow-[0_0_35px_rgba(251,191,36,0.35)]"
          />
        </div>

        {/* Narration & Subtitle Dialogue Box */}
        <div className="relative z-20 max-w-3xl w-full bg-slate-950/85 backdrop-blur-xl border border-amber-500/30 rounded-2xl p-6 md:p-8 shadow-2xl">
          <div className="flex items-center gap-2 mb-3">
            <Sparkles size={16} className="text-amber-400" />
            <h4 className="font-cinzel text-sm font-bold text-amber-200 tracking-wider">
              {scene.characterName}
            </h4>
          </div>

          <p className={`text-slate-200 text-sm md:text-lg leading-relaxed font-sans transition-opacity duration-300 ${
            textVisible ? 'opacity-100' : 'opacity-0'
          }`}>
            {currentSlide.narration}
          </p>

          {currentSlide.dialogue && (
            <p className="mt-3 text-amber-300/90 text-sm md:text-base italic font-serif">
              “{currentSlide.dialogue}”
            </p>
          )}

          {/* Advance Prompt */}
          <div className="mt-5 flex justify-end">
            <button
              onClick={(e) => {
                e.stopPropagation();
                handleNext();
              }}
              className="flex items-center gap-2 px-5 py-2 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs md:text-sm transition shadow-lg"
            >
              <span>{isLastSlide ? 'Conclude Memory' : 'Advance'}</span>
              <ChevronRight size={16} />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
