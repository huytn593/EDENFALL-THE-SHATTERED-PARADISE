import { CharacterRelationship } from '../types';

export const MASTER_CHARACTERS: Record<string, CharacterRelationship> = {
  aurelia: {
    id: 'aurelia',
    name: 'Aurelia',
    title: 'The Solstice Heir',
    role: 'Luminary Seeker & Architect of the Dawn',
    age: 20, // Adult 18+ per mature content framework
    bio: 'The last living heir of the House of Solstice. Raised in seclusion after the Outer Bastions fell, Aurelia carries the weight of a shattered paradise and an unshakeable determination to restore the celestial firmament.',
    location: 'Dawn Garden / The Shattered Spire',
    portrait: '👑',
    favoriteGifts: ['celestial_lily', 'starlight_essence', 'luminary_seal'],
    affection: 35,
    trust: 40,
    familiarity: 30,
    respect: 45,
    comfort: 35,
    conflict: 5,
    stage: 'Trusted Companion',
    currentEmotion: 'gentle',
    schedules: {
      Morning: { locationId: 'loc_garden', activity: 'Tending the celestial lilies and communing with morning starlight', pose: 'kneeling' },
      Afternoon: { locationId: 'loc_spire', activity: 'Inspecting the Astral Core fracture lines and tuning frequencies', pose: 'standing' },
      Evening: { locationId: 'loc_balcony', activity: 'Watching the floating archipelago drift across the sunset void', pose: 'pensive' },
      Night: { locationId: 'loc_spire', activity: 'Resting within the inner sanctuary chamber beneath ward lights', pose: 'sleeping' }
    },
    memories: [
      {
        id: 'mem_aur_init',
        text: 'Awakened the Echo from centuries of celestial slumber.',
        timestamp: 'Day 1 - Dawn',
        impact: 'positive'
      }
    ]
  },
  teresa: {
    id: 'teresa',
    name: 'Commander Teresa',
    title: 'Iron Warden of the Vanguard',
    role: 'Bastion Commander & Military Protector',
    age: 26, // Adult 18+
    bio: 'Battle-hardened leader of the surviving Vanguard battalion. Teresa believes the world was destroyed by reckless sorcerers and values steel, tactical discipline, and relentless sacrifice above all else.',
    location: 'Vanguard Bastion',
    portrait: '🛡️',
    favoriteGifts: ['starmetal_ingot', 'vanguard_aegis', 'astral_draught'],
    affection: 12,
    trust: 25,
    familiarity: 15,
    respect: 50,
    comfort: 18,
    conflict: 15,
    stage: 'Acquaintance',
    currentEmotion: 'serious',
    schedules: {
      Morning: { locationId: 'loc_bastion', activity: 'Drilling the sentries and inspecting the western palisades', pose: 'commanding' },
      Afternoon: { locationId: 'loc_bastion', activity: 'Reviewing reconnaissance reports on abyssal miasma surges', pose: 'deliberating' },
      Evening: { locationId: 'loc_bastion', activity: 'Sharpening her broadsword beside the guard watchfire', pose: 'vigilant' },
      Night: { locationId: 'loc_bastion', activity: 'Patrolling the northern breach battlements in darkness', pose: 'patrolling' }
    },
    memories: [
      {
        id: 'mem_ter_init',
        text: 'Challenged the Echo’s credentials at the Vanguard perimeter.',
        timestamp: 'Day 1 - Morning',
        impact: 'neutral'
      }
    ]
  },
  vaelen: {
    id: 'vaelen',
    name: 'Archivist Vaelen',
    title: 'Curator of the Celestial Vaults',
    role: 'Elder Astrologer & Keeper of Shards',
    age: 42, // Adult 18+
    bio: 'A dedicated scholar who has spent his life transcribing dying starlight. Vaelen operates the Starlight Astrolabe and possesses knowledge of ancient pre-cataclysm mechanisms.',
    location: 'Celestial Archive',
    portrait: '🧙‍♂️',
    favoriteGifts: ['leyline_shard', 'chronos_parchment', 'miniature_chronos'],
    affection: 20,
    trust: 45,
    familiarity: 35,
    respect: 60,
    comfort: 40,
    conflict: 0,
    stage: 'Trusted Companion',
    currentEmotion: 'introspective',
    schedules: {
      Morning: { locationId: 'loc_archive', activity: 'Calibrating the brass Astrolabe rings', pose: 'calibrating' },
      Afternoon: { locationId: 'loc_archive', activity: 'Restoring crumbling vellum manuscripts', pose: 'writing' },
      Evening: { locationId: 'loc_archive', activity: 'Mapping the approaching Eclipse alignment', pose: 'observing' },
      Night: { locationId: 'loc_archive', activity: 'Deep in contemplation beneath the astral ceiling', pose: 'studying' }
    },
    memories: [
      {
        id: 'mem_vae_init',
        text: 'Consulted the Echo regarding the harmonic frequency of the First Dawn.',
        timestamp: 'Day 1 - Afternoon',
        impact: 'positive'
      }
    ]
  },
  sylvara: {
    id: 'sylvara',
    name: 'Sylvara',
    title: 'Voice of the Sunken Grove',
    role: 'Resonant Sylvan Entity',
    age: 22, // Adult 18+
    bio: 'An ethereal dryad-like spirit born from the root system of Edenfall’s sacred canopies. Sylvara remembers the lullabies sung to the stars before the skies fractured.',
    location: 'Sunken Grove',
    portrait: '🔮',
    favoriteGifts: ['astral_dew', 'aetherial_flora', 'celestial_lily'],
    affection: 28,
    trust: 35,
    familiarity: 22,
    respect: 30,
    comfort: 45,
    conflict: 2,
    stage: 'Acquaintance',
    currentEmotion: 'smiling',
    schedules: {
      Morning: { locationId: 'loc_grove', activity: 'Gathering phosphorescent dew from lily pads', pose: 'gathering' },
      Afternoon: { locationId: 'loc_grove', activity: 'Singing gentle melodies to the subterranean moss', pose: 'singing' },
      Evening: { locationId: 'loc_grove', activity: 'Drifting across the Whispering Pool as mist rises', pose: 'floating' },
      Night: { locationId: 'loc_grove', activity: 'Slumbering within the hollow of the Great Celestial Willow', pose: 'resting' }
    },
    memories: [
      {
        id: 'mem_syl_init',
        text: 'Greeted the Echo with ancient forest chimes.',
        timestamp: 'Day 1 - Evening',
        impact: 'positive'
      }
    ]
  }
};
