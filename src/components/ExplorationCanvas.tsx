import React, { useEffect, useRef, useState, useCallback } from 'react';
import { GameState, MapLocation, VisionLayer } from '../types';
import { sound } from '../utils/audio';

interface ExplorationCanvasProps {
  gameState: GameState;
  onSelectLocation: (loc: MapLocation) => void;
  onInteractNode: (nodeId: string) => void;
  onUpdatePlayerPos: (x: number, y: number, staminaDelta: number) => void;
}

export const ExplorationCanvas: React.FC<ExplorationCanvasProps> = ({
  gameState,
  onSelectLocation,
  onInteractNode,
  onUpdatePlayerPos
}) => {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const [nearbyInteractive, setNearbyInteractive] = useState<{ id: string; name: string; type: string } | null>(null);
  const [keysPressed, setKeysPressed] = useState<Record<string, boolean>>({});

  // Locations in Dawn Sanctuary
  const locations: MapLocation[] = [
    {
      id: 'loc_spire',
      name: 'The Shattered Spire',
      subtitle: 'Sanctuary Epicenter',
      type: 'sanctuary',
      x: 500,
      y: 350,
      icon: '🏛️',
      description: 'The monumental core of Dawn Sanctuary, where ancient floating stones orbit a radiant sun-crystal.',
      accessible: true,
      interactiveObjects: [
        { id: 'obj_core', name: 'Astral Core', description: 'Pulses with solar harmonics. Touch to attune.', icon: '🌟', discovered: true },
        { id: 'obj_inscription', name: 'Solstice Inscription', description: 'Ancient text decipherable only with Magical Aura.', icon: '📜', requiresVision: 'magical_aura', discovered: false }
      ]
    },
    {
      id: 'loc_garden',
      name: 'Dawn Garden',
      subtitle: 'Aurelia’s Solace',
      type: 'garden',
      x: 680,
      y: 280,
      icon: '🌸',
      description: 'A serene terrace filled with blooming celestial lilies, overlooking the fractured sky.',
      accessible: true,
      interactiveObjects: [
        { id: 'obj_flower', name: 'Celestial Lily', description: 'A rare flower that radiates gentle starlight.', icon: '🌺', discovered: true }
      ]
    },
    {
      id: 'loc_archive',
      name: 'Celestial Archive',
      subtitle: 'Vaelen’s Vault',
      type: 'archive',
      x: 500,
      y: 130,
      icon: '📜',
      description: 'Towering shelves of illuminated manuscripts and the great bronze Starlight Astrolabe.',
      accessible: true,
      interactiveObjects: [
        { id: 'obj_astrolabe', name: 'Starlight Astrolabe', description: 'Calculates the trajectory of celestial debris.', icon: '🧭', discovered: true }
      ]
    },
    {
      id: 'loc_bastion',
      name: 'Vanguard Bastion',
      subtitle: 'Teresa’s Redoubt',
      type: 'bastion',
      x: 170,
      y: 350,
      icon: '⚔️',
      description: 'Formidable iron-reinforced ramparts defending the western approach against the encroaching dark.',
      accessible: true,
      interactiveObjects: [
        { id: 'obj_weapon_rack', name: 'Starmetal Armory', description: 'Tempered blades forged to cut through miasma.', icon: '🛡️', discovered: true }
      ]
    },
    {
      id: 'loc_grove',
      name: 'Sunken Grove',
      subtitle: 'Whispering Pool',
      type: 'grove',
      x: 820,
      y: 370,
      icon: '🌿',
      description: 'A bioluminescent hollow beneath ancient crystal weeping willows, sacred to the Sylph Chorus.',
      accessible: true,
      interactiveObjects: [
        { id: 'obj_pool', name: 'Moonlit Font', description: 'Reflects constellations that perished in the Shattering.', icon: '💧', discovered: true }
      ]
    },
    {
      id: 'loc_balcony',
      name: 'Moonlit Balcony',
      subtitle: 'Stargazer’s Perch',
      type: 'balcony',
      x: 640,
      y: 500,
      icon: '✨',
      description: 'A secluded marble overlook providing an intimate, breathtaking panoramic vista of Edenfall’s floating archipelago.',
      accessible: true,
      interactiveObjects: [
        { id: 'obj_telescope', name: 'Aetherial Lens', description: 'Permits viewing distant drifting islands in the void.', icon: '🔭', discovered: true }
      ]
    },
    {
      id: 'loc_gate',
      name: 'The Eclipse Gate',
      subtitle: 'Southern Barrier',
      type: 'gate',
      x: 480,
      y: 580,
      icon: '🌌',
      description: 'A massive circular arch sealed by ancient ward-locks. The Abyssal Wastes lie beyond.',
      accessible: true,
      interactiveObjects: [
        { id: 'obj_ward_rune', name: 'Ward Keystone', description: 'Glows faintly, holding back the miasma.', icon: '🔒', discovered: true }
      ]
    }
  ];

  // NPC current locations depending on time of day per Master Directive
  const time = gameState.world.timeOfDay;
  const npcsAtTime: { id: string; name: string; icon: string; x: number; y: number }[] = [];

  // Aurelia schedule: Morning at Garden, Afternoon at Spire, Evening at Balcony, Night in Sanctuary Chamber
  if (time === 'Morning') npcsAtTime.push({ id: 'aurelia', name: 'Aurelia (in Garden)', icon: '👑', x: 670, y: 300 });
  else if (time === 'Afternoon') npcsAtTime.push({ id: 'aurelia', name: 'Aurelia (at Spire)', icon: '👑', x: 530, y: 370 });
  else if (time === 'Evening') npcsAtTime.push({ id: 'aurelia', name: 'Aurelia (on Balcony)', icon: '👑', x: 650, y: 520 });
  else npcsAtTime.push({ id: 'aurelia', name: 'Aurelia (Resting at Spire)', icon: '👑', x: 480, y: 340 });

  // Teresa schedule: Bastion defense
  npcsAtTime.push({ id: 'teresa', name: 'Commander Teresa', icon: '🛡️', x: 190, y: 370 });

  // Vaelen schedule: Archive
  npcsAtTime.push({ id: 'vaelen', name: 'Archivist Vaelen', icon: '🧙‍♂️', x: 490, y: 150 });

  // Sylvara schedule: Sunken Grove
  npcsAtTime.push({ id: 'sylvara', name: 'The Resonant Echo', icon: '🔮', x: 800, y: 380 });

  // Handle keyboard movement
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      setKeysPressed(prev => ({ ...prev, [e.key.toLowerCase()]: true }));
      if (e.key.toLowerCase() === 'e' && nearbyInteractive) {
        onInteractNode(nearbyInteractive.id);
        sound.playClick();
      }
    };
    const handleKeyUp = (e: KeyboardEvent) => {
      setKeysPressed(prev => ({ ...prev, [e.key.toLowerCase()]: false }));
    };

    window.addEventListener('keydown', handleKeyDown);
    window.addEventListener('keyup', handleKeyUp);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
      window.removeEventListener('keyup', handleKeyUp);
    };
  }, [nearbyInteractive, onInteractNode]);

  // Click to move target
  const handleCanvasClick = (e: React.MouseEvent<HTMLCanvasElement>) => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;
    const targetX = (e.clientX - rect.left) * scaleX;
    const targetY = (e.clientY - rect.top) * scaleY;

    // Check if clicked near an interactive node
    for (const loc of locations) {
      const dist = Math.hypot(loc.x - targetX, loc.y - targetY);
      if (dist < 45) {
        onSelectLocation(loc);
        sound.playClick();
        return;
      }
    }

    for (const npc of npcsAtTime) {
      const dist = Math.hypot(npc.x - targetX, npc.y - targetY);
      if (dist < 40) {
        onInteractNode(npc.id);
        sound.playClick();
        return;
      }
    }

    // Move player towards click
    onUpdatePlayerPos(targetX, targetY, 0);
  };

  // Main 60FPS animation & render loop
  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    let animationFrameId: number;
    let particleTime = 0;

    const render = () => {
      particleTime += 0.02;

      // Handle keyboard smooth movement
      let dx = 0;
      let dy = 0;
      const speed = keysPressed['shift'] ? 4.5 : 2.6;

      if (keysPressed['w'] || keysPressed['arrowup']) dy -= speed;
      if (keysPressed['s'] || keysPressed['arrowdown']) dy += speed;
      if (keysPressed['a'] || keysPressed['arrowleft']) dx -= speed;
      if (keysPressed['d'] || keysPressed['arrowright']) dx += speed;

      if (dx !== 0 || dy !== 0) {
        const newX = Math.max(50, Math.min(canvas.width - 50, gameState.player.x + dx));
        const newY = Math.max(50, Math.min(canvas.height - 50, gameState.player.y + dy));
        const staminaCost = keysPressed['shift'] ? -0.15 : 0;
        onUpdatePlayerPos(newX, newY, staminaCost);
      }

      // Check proximity for nearest interactive node or NPC
      let closest: { id: string; name: string; type: string } | null = null;
      let minDistance = 55;

      for (const npc of npcsAtTime) {
        const d = Math.hypot(npc.x - gameState.player.x, npc.y - gameState.player.y);
        if (d < minDistance) {
          minDistance = d;
          closest = { id: npc.id, name: npc.name, type: 'npc' };
        }
      }

      for (const loc of locations) {
        const d = Math.hypot(loc.x - gameState.player.x, loc.y - gameState.player.y);
        if (d < minDistance) {
          minDistance = d;
          closest = { id: loc.id, name: loc.name, type: 'location' };
        }
      }

      setNearbyInteractive(closest);

      // CLEAR & BACKGROUND: Atmospheric sky gradient based on Time of Day
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      let skyTop = '#030712';
      let skyBottom = '#0f172a';
      let ambientColor = 'rgba(251, 191, 36, 0.08)';

      if (time === 'Morning') {
        skyTop = '#0f172a';
        skyBottom = '#1e293b';
        ambientColor = 'rgba(254, 240, 138, 0.12)';
      } else if (time === 'Afternoon') {
        skyTop = '#09152b';
        skyBottom = '#172554';
        ambientColor = 'rgba(251, 191, 36, 0.14)';
      } else if (time === 'Evening') {
        skyTop = '#1e1b4b';
        skyBottom = '#4c1d95';
        ambientColor = 'rgba(244, 114, 182, 0.12)';
      } else if (time === 'Night') {
        skyTop = '#020617';
        skyBottom = '#090d16';
        ambientColor = 'rgba(129, 140, 248, 0.08)';
      }

      const bgGrad = ctx.createLinearGradient(0, 0, 0, canvas.height);
      bgGrad.addColorStop(0, skyTop);
      bgGrad.addColorStop(1, skyBottom);
      ctx.fillStyle = bgGrad;
      ctx.fillRect(0, 0, canvas.width, canvas.height);

      // PARALLAX LAYER 1: Distant Floating Islands & Starlight Nebulae
      ctx.save();
      for (let i = 0; i < 45; i++) {
        const starX = (i * 73 + particleTime * 1.5) % canvas.width;
        const starY = (i * 47) % (canvas.height * 0.7);
        const radius = (i % 3 === 0) ? 1.8 : 1.0;
        const alpha = 0.3 + 0.6 * Math.sin(particleTime * 2 + i);
        ctx.fillStyle = `rgba(254, 243, 199, ${alpha})`;
        ctx.beginPath();
        ctx.arc(starX, starY, radius, 0, Math.PI * 2);
        ctx.fill();
      }
      ctx.restore();

      // PARALLAX LAYER 2: Floating Celestial Archipelago silhouettes
      ctx.save();
      ctx.fillStyle = 'rgba(30, 41, 59, 0.4)';
      ctx.beginPath();
      ctx.ellipse(220, 180, 120, 35, 0.08, 0, Math.PI * 2);
      ctx.ellipse(780, 210, 140, 40, -0.05, 0, Math.PI * 2);
      ctx.fill();
      ctx.restore();

      // LAYER 3: Sanctuary Stone Flagstones & Leyline Networks
      ctx.save();
      // Draw Leylines connecting Spire to all key locations
      const spire = locations[0];
      locations.slice(1).forEach((loc, idx) => {
        ctx.beginPath();
        ctx.moveTo(spire.x, spire.y);
        // Subtle curve
        const midX = (spire.x + loc.x) / 2 + Math.sin(particleTime + idx) * 15;
        const midY = (spire.y + loc.y) / 2 + Math.cos(particleTime + idx) * 15;
        ctx.quadraticCurveTo(midX, midY, loc.x, loc.y);

        if (gameState.world.activeVisionLayer === 'magical_aura' || gameState.world.activeVisionLayer === 'energy_pathways') {
          ctx.strokeStyle = 'rgba(56, 189, 248, 0.75)';
          ctx.lineWidth = 3.5;
          ctx.setLineDash([8, 8]);
          ctx.lineDashOffset = -particleTime * 20;
        } else {
          ctx.strokeStyle = 'rgba(217, 119, 6, 0.25)';
          ctx.lineWidth = 2;
          ctx.setLineDash([]);
        }
        ctx.stroke();
      });
      ctx.restore();

      // LAYER 4: Draw Interactive Locations
      locations.forEach(loc => {
        const isHovered = nearbyInteractive?.id === loc.id;
        const pulse = Math.sin(particleTime * 3) * 3;

        // Base sanctuary stone platform
        ctx.save();
        ctx.fillStyle = isHovered ? 'rgba(51, 65, 85, 0.9)' : 'rgba(30, 41, 59, 0.7)';
        ctx.strokeStyle = isHovered ? '#fbbf24' : 'rgba(148, 163, 184, 0.4)';
        ctx.lineWidth = isHovered ? 2.5 : 1.5;
        ctx.beginPath();
        ctx.ellipse(loc.x, loc.y + 12, 34 + (isHovered ? pulse : 0), 18 + (isHovered ? pulse * 0.5 : 0), 0, 0, Math.PI * 2);
        ctx.fill();
        ctx.stroke();

        // Glowing Rune Ring
        ctx.beginPath();
        ctx.arc(loc.x, loc.y, 22 + (isHovered ? 4 : 0), 0, Math.PI * 2);
        ctx.strokeStyle = isHovered ? 'rgba(251, 191, 36, 0.8)' : 'rgba(125, 211, 252, 0.4)';
        ctx.lineWidth = 1.5;
        ctx.stroke();

        // Location Icon
        ctx.font = '22px sans-serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(loc.icon, loc.x, loc.y - 4);

        // Location Name Label
        ctx.font = isHovered ? 'bold 13px "Cinzel", serif' : '11px "Cinzel", serif';
        ctx.fillStyle = isHovered ? '#fef08a' : '#cbd5e1';
        ctx.fillText(loc.name, loc.x, loc.y + 36);
        ctx.restore();
      });

      // LAYER 5: Draw NPCs
      npcsAtTime.forEach(npc => {
        const isNearby = nearbyInteractive?.id === npc.id;
        const bob = Math.sin(particleTime * 3.5) * 2;

        ctx.save();
        // NPC shadow
        ctx.fillStyle = 'rgba(0, 0, 0, 0.4)';
        ctx.beginPath();
        ctx.ellipse(npc.x, npc.y + 14, 16, 6, 0, 0, Math.PI * 2);
        ctx.fill();

        // NPC Aura
        ctx.beginPath();
        ctx.arc(npc.x, npc.y - 10 + bob, isNearby ? 22 : 18, 0, Math.PI * 2);
        ctx.fillStyle = isNearby ? 'rgba(251, 191, 36, 0.25)' : 'rgba(192, 132, 252, 0.15)';
        ctx.fill();

        // NPC Sprite/Icon
        ctx.font = '26px sans-serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(npc.icon, npc.x, npc.y - 12 + bob);

        // NPC Name Tag
        ctx.font = '12px "Plus Jakarta Sans", sans-serif';
        ctx.fillStyle = isNearby ? '#fef08a' : '#f1f5f9';
        ctx.fillText(npc.name, npc.x, npc.y + 26);
        ctx.restore();
      });

      // LAYER 6: Draw Player (Aurelia / Echo)
      ctx.save();
      const px = gameState.player.x;
      const py = gameState.player.y;
      const isSprinting = keysPressed['shift'];
      const playerPulse = Math.sin(particleTime * 4) * 3;

      // Player shadow
      ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
      ctx.beginPath();
      ctx.ellipse(px, py + 14, 18, 7, 0, 0, Math.PI * 2);
      ctx.fill();

      // Celestial Luminary Aura surrounding Aurelia
      const auraGradient = ctx.createRadialGradient(px, py - 10, 5, px, py - 10, 36);
      auraGradient.addColorStop(0, 'rgba(253, 224, 71, 0.5)');
      auraGradient.addColorStop(0.6, 'rgba(245, 158, 11, 0.2)');
      auraGradient.addColorStop(1, 'transparent');
      ctx.fillStyle = auraGradient;
      ctx.beginPath();
      ctx.arc(px, py - 10, 36 + playerPulse, 0, Math.PI * 2);
      ctx.fill();

      // Aurelia Avatar
      ctx.font = '28px sans-serif';
      ctx.textAlign = 'center';
      ctx.textBaseline = 'middle';
      ctx.fillText('✨', px, py - 12);

      // Player Name & Title
      ctx.font = 'bold 12px "Cinzel", serif';
      ctx.fillStyle = '#fef08a';
      ctx.fillText(gameState.player.name, px, py + 26);
      ctx.font = '10px "Plus Jakarta Sans", sans-serif';
      ctx.fillStyle = '#94a3b8';
      ctx.fillText(gameState.player.title, px, py + 38);
      ctx.restore();

      // Ambient celestial dust motes
      ctx.save();
      for (let j = 0; j < 25; j++) {
        const moteX = (j * 43 + Math.sin(particleTime + j) * 40) % canvas.width;
        const moteY = (j * 31 + Math.cos(particleTime + j) * 40) % canvas.height;
        ctx.fillStyle = 'rgba(253, 224, 71, 0.35)';
        ctx.beginPath();
        ctx.arc(moteX, moteY, 1.2, 0, Math.PI * 2);
        ctx.fill();
      }
      ctx.restore();

      animationFrameId = requestAnimationFrame(render);
    };

    render();

    return () => {
      cancelAnimationFrame(animationFrameId);
    };
  }, [gameState, keysPressed, nearbyInteractive, npcsAtTime, time, onSelectLocation, onInteractNode, onUpdatePlayerPos]);

  return (
    <div className="relative w-full h-[540px] md:h-[620px] rounded-2xl overflow-hidden border border-slate-800 bg-slate-950 shadow-2xl">
      {/* 2.5D Interactive Canvas */}
      <canvas
        ref={canvasRef}
        width={1000}
        height={650}
        onClick={handleCanvasClick}
        className="w-full h-full cursor-crosshair object-cover"
      />

      {/* Proximity Interaction Prompt */}
      {nearbyInteractive && (
        <div className="absolute bottom-6 left-1/2 transform -translate-x-1/2 bg-slate-900/90 backdrop-blur-md px-6 py-2.5 rounded-full border border-amber-500/60 shadow-xl flex items-center gap-3 animate-bounce">
          <span className="px-2 py-0.5 rounded bg-amber-500/20 text-amber-300 font-mono text-xs font-bold border border-amber-500/40">
            E
          </span>
          <span className="text-sm font-cinzel text-slate-100">
            Interact with <span className="text-amber-300 font-semibold">{nearbyInteractive.name}</span>
          </span>
          <button
            onClick={() => onInteractNode(nearbyInteractive.id)}
            className="px-3 py-1 rounded bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs transition"
          >
            Interact
          </button>
        </div>
      )}

      {/* Navigation & Controls Overlay Instructions */}
      <div className="absolute top-4 left-4 bg-slate-950/70 backdrop-blur-md px-3.5 py-2 rounded-xl border border-slate-800 text-xs text-slate-300 flex items-center gap-4">
        <div>
          <span className="font-semibold text-amber-400">Controls:</span> Move with <kbd className="px-1.5 py-0.5 bg-slate-800 rounded font-mono text-slate-200">WASD</kbd> or <span className="text-amber-200">Click Map</span>
        </div>
        <div className="h-3 w-px bg-slate-700" />
        <div>
          Sprint: <kbd className="px-1.5 py-0.5 bg-slate-800 rounded font-mono text-slate-200">Shift</kbd>
        </div>
        <div className="h-3 w-px bg-slate-700" />
        <div>
          Interact: <kbd className="px-1.5 py-0.5 bg-slate-800 rounded font-mono text-slate-200">E</kbd>
        </div>
      </div>
    </div>
  );
};
