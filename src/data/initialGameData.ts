import { Item, Quest } from '../types';

export const INITIAL_ITEMS: Item[] = [
  {
    id: 'luminary_seal',
    name: 'Crest of the Luminary',
    category: 'key_item',
    rarity: 'celestial',
    description: 'An ancient medallion of woven starlight, etched with the sigil of Edenfall.',
    lore: 'Passed down through the bloodline of the House of Solstice. It pulses in rhythm with the celestial leylines of the sanctuary.',
    icon: '✨',
    quantity: 1,
    usable: true,
    effectDescription: 'Purifies 20 Abyssal Corruption and attunes to celestial shrines.'
  },
  {
    id: 'celestial_lily',
    name: 'Dawn Celestial Lily',
    category: 'gift',
    rarity: 'rare',
    description: 'A delicate glowing blossom harvested from the Dawn Garden terrace.',
    lore: 'Aurelia’s favorite flower. Said to bloom only under pure uncorrupted starlight.',
    icon: '🌸',
    quantity: 2,
    usable: false,
    giftTarget: 'aurelia'
  },
  {
    id: 'leyline_shard',
    name: 'Dawn Leyline Shard',
    category: 'material',
    rarity: 'rare',
    description: 'A crystalline fragment glowing with warm amber solar energy.',
    lore: 'Residual resonance crystallized when the Paradise firmament shattered three centuries ago.',
    icon: '💎',
    quantity: 3,
    usable: false
  },
  {
    id: 'starmetal_ingot',
    name: 'Vanguard Starmetal Ingot',
    category: 'gift',
    rarity: 'rare',
    description: 'Dense metal forged in the Bastion furnaces to reinforce armor against miasma.',
    lore: 'Commander Teresa values refined starmetal above all philosophical relics.',
    icon: '🛡️',
    quantity: 1,
    usable: false,
    giftTarget: 'teresa'
  },
  {
    id: 'astral_draught',
    name: 'Astral Nectar Draught',
    category: 'consumable',
    rarity: 'common',
    description: 'Brewed from bioluminescent moss native to the Sunken Grove.',
    lore: 'Restores stamina and grants clarity of thought during exploration.',
    icon: '🧪',
    quantity: 3,
    usable: true,
    effectDescription: 'Restores 40 Stamina immediately.'
  },
  {
    id: 'chronos_parchment',
    name: 'Vaelen’s Starmap Cipher',
    category: 'relic',
    rarity: 'ancient',
    description: 'Vellum paper inscribed with celestial orbits that no longer match the night sky.',
    lore: 'Calculations attempting to forecast the next Great Eclipse and the rupture of the sanctuary wards.',
    icon: '📜',
    quantity: 1,
    usable: false,
    giftTarget: 'vaelen'
  }
];

export const INITIAL_QUESTS: Quest[] = [
  {
    id: 'quest_spire',
    title: 'The Shattered Awakening',
    summary: 'The Dawn Sanctuary’s central spire emits a low harmonic frequency. Journey to the heart of the ruins and attune the Astral Core.',
    category: 'main',
    status: 'active',
    currentStepIndex: 0,
    rewardText: 'Celestial Leyline Resonance +15, Starlight Essence x2',
    steps: [
      { description: 'Approach the Shattered Spire at the center of Dawn Sanctuary', completed: false, targetNodeId: 'loc_spire' },
      { description: 'Channel your Luminary Resonance into the slumbering Astral Core', completed: false, targetNodeId: 'loc_spire' }
    ]
  },
  {
    id: 'quest_vaelen',
    title: 'Whispers of the Archivist',
    summary: 'Archivist Vaelen requires ancient Leyline Shards to decipher the encroaching Eclipse Cycle before the sky falls once more.',
    category: 'sanctuary',
    status: 'active',
    currentStepIndex: 0,
    rewardText: 'Ancient Relic: Chronos Monolith & Archivist Trust +25',
    steps: [
      { description: 'Locate Archivist Vaelen at the Celestial Archive (North)', completed: false, targetNodeId: 'loc_archive' },
      { description: 'Offer 1 Dawn Leyline Shard to power the Celestial Astrolabe', completed: false, targetNodeId: 'loc_archive' }
    ]
  },
  {
    id: 'quest_teresa',
    title: 'The Vanguard’s Ultimatum',
    summary: 'Commander Teresa believes outsiders bring corruption. Parley with her at the Bastion and prove your defensive resolve.',
    category: 'character',
    status: 'active',
    currentStepIndex: 0,
    rewardText: 'Vanguard Aegis Insignia & Bastion Alliance',
    steps: [
      { description: 'Travel to the Vanguard Bastion (West)', completed: false, targetNodeId: 'loc_bastion' },
      { description: 'Address Teresa’s suspicions without escalating hostilities', completed: false, targetNodeId: 'loc_bastion' }
    ]
  },
  {
    id: 'quest_garden_solace',
    title: 'Moonlit Sanctuary Solace',
    summary: 'Aurelia visits the Dawn Garden when evening approaches. Spend time together amidst the celestial lilies.',
    category: 'character',
    status: 'active',
    currentStepIndex: 0,
    rewardText: 'Unlock Cinematic CG: “Moonlit Sanctuary Solace” & Deepen Bond',
    steps: [
      { description: 'Advance time to Evening or Night and meet Aurelia at the Dawn Garden', completed: false, targetNodeId: 'loc_garden' },
      { description: 'Converse with Aurelia and unlock the intimate terrace memory', completed: false, targetNodeId: 'loc_garden' }
    ]
  }
];
