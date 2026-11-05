import { CinematicScene } from '../types';

export const CINEMATIC_SCENES: Record<string, CinematicScene> = {
  cg_aurelia_garden: {
    id: 'cg_aurelia_garden',
    title: 'Moonlit Sanctuary Solace',
    characterId: 'aurelia',
    characterName: 'Aurelia',
    description: 'An intimate evening in the Dawn Garden with Aurelia beneath cascading celestial lilies and the shattered constellations.',
    unlocked: false,
    unlockConditions: {
      minAffection: 20,
      locationId: 'loc_garden'
    },
    slides: [
      {
        characterEmotion: 'introspective',
        narration: 'The night air in the Dawn Garden is thick with the scent of blooming celestial lilies. Aurelia stands near the marble balustrade, watching the distant floating spires drift in the starlight.',
        dialogue: 'Every time I look into the void, I wonder if the world was truly meant to be saved... or if it is merely exhausted.',
        cameraEffect: 'pan_up',
        cgImagePrompt: 'Aurelia looking over the moonlit sanctuary garden'
      },
      {
        characterEmotion: 'flustered',
        narration: 'As you step beside her, your fingers lightly brush against the soft silk of her sleeve. A subtle tremor passes through her breath, yet she does not draw away.',
        dialogue: 'You have that quiet resolve, Echo. In a world full of shattering firmaments and shouting warlords, your presence is... soothing.',
        cameraEffect: 'soft_glow',
        cgImagePrompt: 'Close up of Aurelia blushing under gentle starlight'
      },
      {
        characterEmotion: 'radiant',
        narration: 'She turns to face you completely. The celestial tiara in her golden hair glints with pure solar warmth. She extends her slender hand, entwining her fingers with yours.',
        dialogue: 'Promise me one thing, Echo. Whatever happens when the Eclipse Gate opens... you will stay by my side.',
        cameraEffect: 'radiance',
        cgImagePrompt: 'Aurelia smiling radiantly and holding hands'
      },
      {
        characterEmotion: 'warm',
        narration: 'The two of you stand together in the sanctuary garden as soft petals drift across the terrace, bound by a mutual vow that transcends the fall of paradise.',
        dialogue: 'Thank you for awakening, my Echo. For the first time in centuries, I feel hope.',
        cameraEffect: 'fade_black',
        cgImagePrompt: 'Tender embrace in the starlight garden'
      }
    ],
    rewards: {
      affectionBonus: 15,
      trustBonus: 15,
      memoryText: 'Shared an intimate night beneath the celestial lilies with Aurelia.'
    }
  },
  cg_spire_awakening: {
    id: 'cg_spire_awakening',
    title: 'Rekindling of the First Dawn',
    characterId: 'aurelia',
    characterName: 'Aurelia',
    description: 'The monumental moment when Aurelia and the Echo attune their soul frequencies to the Astral Core of the Shattered Spire.',
    unlocked: false,
    unlockConditions: {
      minTrust: 25,
      locationId: 'loc_spire'
    },
    slides: [
      {
        characterEmotion: 'serious',
        narration: 'The floating monolith fragments above the Shattered Spire begin to rotate in sacred geometric harmony. The ground reverberates with a deep sub-bass hum.',
        dialogue: 'This is the moment our ancestors failed to complete. Focus your intent, Echo!',
        cameraEffect: 'zoom_in',
        cgImagePrompt: 'Monolith fragments rotating in gold starlight'
      },
      {
        characterEmotion: 'radiant',
        narration: 'A radiant pillar of crystalline starlight erupts through the firmament, scattering the dark clouds into luminous ribbons of aurora borealis!',
        dialogue: 'Look! The northern ward responds! Dawn Sanctuary is breathing once more!',
        cameraEffect: 'radiance',
        cgImagePrompt: 'Pillar of starlight piercing the heavens'
      }
    ],
    rewards: {
      affectionBonus: 10,
      trustBonus: 20,
      memoryText: 'Rekindled the central Astral Core of Dawn Sanctuary.'
    }
  },
  cg_teresa_oath: {
    id: 'cg_teresa_oath',
    title: 'The Warden’s Unyielding Vigil',
    characterId: 'teresa',
    characterName: 'Commander Teresa',
    description: 'Commander Teresa lowers her iron guard and confesses the cost of holding the Vanguard Bastion against three centuries of encroaching dark.',
    unlocked: false,
    unlockConditions: {
      minTrust: 30,
      locationId: 'loc_bastion'
    },
    slides: [
      {
        characterEmotion: 'serious',
        narration: 'Wind howls across the battlements of the Vanguard Bastion. Teresa unbuckles her heavy starmetal gorget, revealing scars etched across her collarbone.',
        dialogue: 'People call me the Iron Warden because they need to believe iron doesn’t bleed. But every sentry lost out there is a name I whisper before sleeping.',
        cameraEffect: 'soft_glow',
        cgImagePrompt: 'Commander Teresa on the ramparts under stormy skies'
      },
      {
        characterEmotion: 'smiling',
        narration: 'For the first time since you met her, a rare, genuine smile softens the harsh angles of her warrior countenance.',
        dialogue: 'Perhaps... I do not have to carry this wall entirely alone anymore. I accept your alliance, Echo.',
        cameraEffect: 'fade_black',
        cgImagePrompt: 'Teresa smiling warmly on the battlements'
      }
    ],
    rewards: {
      affectionBonus: 12,
      trustBonus: 25,
      memoryText: 'Earned the true loyalty and respect of Commander Teresa.'
    }
  }
};
