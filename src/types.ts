export type TimeOfDay = 'Morning' | 'Afternoon' | 'Evening' | 'Night';
export type WeatherType = 'Celestial Clear' | 'Astral Aurora' | 'Starlight Mist' | 'Eclipse Surge';
export type VisionLayer = 'normal' | 'magical_aura' | 'energy_pathways' | 'celestial_silhouette' | 'miasma_sight';

export type RelationshipStage =
  | 'Stranger'
  | 'Acquaintance'
  | 'Trusted Companion'
  | 'Emotional Connection'
  | 'Romantic Possibility'
  | 'Established Relationship'
  | 'Personal Story Resolution';

export interface CharacterEmotion {
  id: string;
  name: string;
  avatarSvg: string;
  expression: string;
}

export interface CharacterRelationship {
  id: string;
  name: string;
  title: string;
  role: string;
  age: number; // Strictly 18+ adult characters per directive
  bio: string;
  location: string;
  portrait: string;
  favoriteGifts: string[];
  // Independent relationship dimensions per directive 4.2
  affection: number; // 0 to 100
  trust: number;     // 0 to 100
  familiarity: number; // 0 to 100
  respect: number;   // 0 to 100
  comfort: number;   // 0 to 100
  conflict: number;  // 0 to 100
  affinity?: number;
  suspicion?: number;
  status?: string;
  stage: RelationshipStage;
  currentEmotion: string;
  schedules: Record<TimeOfDay, { locationId: string; activity: string; pose: string }>;
  memories: {
    id: string;
    text: string;
    timestamp: string;
    impact: 'positive' | 'negative' | 'neutral' | 'romantic';
  }[];
}

export type DialogueTone =
  | 'romantic'
  | 'inquisitive'
  | 'compassionate'
  | 'defiant'
  | 'cautious'
  | 'mystical'
  | 'flirtatious';

export interface DialogueChoice {
  id: string;
  text: string;
  tone?: DialogueTone;
  reqItem?: string;
  reqAffection?: number;
  reqTrust?: number;
  reqAffinity?: { characterId: string; min: number };
  affectionDelta?: number;
  trustDelta?: number;
  respectDelta?: number;
  comfortDelta?: number;
  conflictDelta?: number;
  affinityDelta?: number;
  memoryText?: string;
  consequenceText?: string;
  triggerEventId?: string;
  questTrigger?: { questId: string; stepIndex: number };
  rewardItem?: Item;
  itemReward?: Item;
  worldResonanceDelta?: number;
  nextNodeId?: string | null;
}

export interface DialogueNode {
  id: string;
  speaker: string;
  speakerId?: string;
  speakerTitle?: string;
  speakerPortrait?: string;
  emotion?: string;
  text: string;
  choices: DialogueChoice[];
}

export interface DialogueTree {
  id: string;
  characterId: string;
  initialNodeId: string;
  nodes: Record<string, DialogueNode>;
}

export interface Item {
  id: string;
  name: string;
  category: 'relic' | 'material' | 'consumable' | 'key_item' | 'gift';
  rarity: 'common' | 'rare' | 'celestial' | 'ancient';
  description: string;
  lore: string;
  icon: string;
  quantity: number;
  usable?: boolean;
  giftTarget?: string;
  effectDescription?: string;
}

export interface CinematicScene {
  id: string;
  title: string;
  characterId: string;
  characterName: string;
  description: string;
  unlocked: boolean;
  unlockConditions: {
    minAffection?: number;
    minTrust?: number;
    timeOfDay?: TimeOfDay;
    locationId?: string;
  };
  slides: {
    characterEmotion: string;
    narration: string;
    dialogue?: string;
    cameraEffect: 'pan_up' | 'zoom_in' | 'soft_glow' | 'fade_black' | 'radiance';
    cgImagePrompt: string;
  }[];
  rewards: {
    affectionBonus: number;
    trustBonus: number;
    memoryText: string;
  };
}

export interface QuestStep {
  description: string;
  completed: boolean;
  targetNodeId?: string;
}

export interface Quest {
  id: string;
  title: string;
  summary: string;
  category: 'main' | 'character' | 'exploration' | 'sanctuary';
  status: 'active' | 'completed' | 'locked';
  characterId?: string;
  steps: QuestStep[];
  currentStepIndex: number;
  rewardText: string;
  rewards?: {
    items?: Item[];
    resonanceBonus?: number;
  };
}

export interface MapLocation {
  id: string;
  name: string;
  subtitle: string;
  type: 'sanctuary' | 'garden' | 'archive' | 'bastion' | 'grove' | 'gate' | 'balcony';
  x: number;
  y: number;
  icon: string;
  description: string;
  accessible: boolean;
  interactiveObjects: {
    id: string;
    name: string;
    description: string;
    icon: string;
    rewardItem?: Item;
    requiresVision?: VisionLayer;
    discovered: boolean;
  }[];
}

export interface MapInteractiveNode {
  id: string;
  name: string;
  type: 'npc' | 'shrine' | 'relic' | 'gate' | 'lore' | 'bastion';
  x: number;
  y: number;
  icon: string;
  description: string;
  characterId?: string;
  dialogueTreeId?: string;
  rewardItem?: Item;
  examined?: boolean;
  requiredItem?: string;
  loreEntryId?: string;
}

export interface LoreEntry {
  id: string;
  category: 'world' | 'characters' | 'magic_leylines' | 'art_design';
  title: string;
  subtitle: string;
  dateOrEra: string;
  content: string[];
  visualPrompt?: string;
}

export interface GameState {
  player: {
    name: string;
    title: string;
    x: number;
    y: number;
    targetX?: number;
    targetY?: number;
    stamina: number;
    maxStamina: number;
    luminaryResonance: number;
    unlockedFastTravel?: string[];
  };
  world: {
    timeOfDay: TimeOfDay;
    dayNumber: number;
    weather: WeatherType;
    currentRegion: string;
    currentLocationId: string;
    activeVisionLayer: VisionLayer;
    sanctuaryRestoration: number; // 0 to 100%
  };
  relationships: Record<string, CharacterRelationship>;
  inventory: Item[];
  quests: Quest[];
  unlockedCGs: string[];
  activeCinematic: CinematicScene | null;
  activeDialogue: {
    characterId: string;
    currentNodeId: string;
  } | null;
  logs: { id: string; time: string; text: string; type: 'event' | 'relationship' | 'quest' | 'item' | 'info' }[];
}
