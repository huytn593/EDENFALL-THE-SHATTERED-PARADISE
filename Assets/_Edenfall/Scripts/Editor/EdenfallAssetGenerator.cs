#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Edenfall.Characters;
using Edenfall.Cinematics;
using Edenfall.Core;
using Edenfall.Dialogue;
using Edenfall.Events;
using Edenfall.Inventory;
using Edenfall.NPC.Scheduling;
using Edenfall.Quests;
using Edenfall.World;
using UnityEditor;
using UnityEngine;

namespace Edenfall.Editor
{
    public static class EdenfallAssetGenerator
    {
        private const string DataRoot = "Assets/_Edenfall/Data";

        [MenuItem("Edenfall/Generate All Production Assets")]
        public static void GenerateAllAssets()
        {
            EnsureDirectories();
            GenerateCharacters();
            GenerateItems();
            GenerateQuests();
            GenerateCinematics();
            GenerateDialogueTrees();
            GenerateSchedules();
            GenerateLocations();
            GenerateMatureEvents();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EdenfallAssetGenerator] All production game data ScriptableObjects generated successfully!");
        }

        private static void EnsureDirectories()
        {
            EnsureDir($"{DataRoot}/Characters");
            EnsureDir($"{DataRoot}/Items");
            EnsureDir($"{DataRoot}/Quests");
            EnsureDir($"{DataRoot}/Cinematics");
            EnsureDir($"{DataRoot}/Dialogue");
            EnsureDir($"{DataRoot}/Schedules");
            EnsureDir($"{DataRoot}/Locations");
            EnsureDir($"{DataRoot}/Events");
        }

        private static void EnsureDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void GenerateCharacters()
        {
            // 1. Aurelia
            var aurelia = ScriptableObject.CreateInstance<CharacterDefinition>();
            aurelia.characterId = "aurelia";
            aurelia.characterName = "Aurelia";
            aurelia.title = "The Solstice Heir";
            aurelia.role = "Luminary Seeker & Architect of the Dawn";
            aurelia.age = 20;
            aurelia.bio = "Người kế vị cuối cùng của Gia Tộc Hạ Chí (House of Solstice). Lớn lên trong thánh điện biệt lập sau khi Tiền Tuyến sụp đổ, Aurelia gánh trên vai trọng trách hồi sinh vầng thái dương của thiên đường vỡ vụn.";
            aurelia.defaultLocation = "Dawn Garden";
            aurelia.favoriteGiftItemIds = new List<string> { "celestial_lily", "starlight_essence", "luminary_seal" };
            aurelia.startingAffection = 35;
            aurelia.startingTrust = 40;
            aurelia.startingFamiliarity = 30;
            aurelia.startingRespect = 45;
            aurelia.startingComfort = 35;
            aurelia.startingConflict = 5;
            SaveAsset(aurelia, $"{DataRoot}/Characters/Character_Aurelia.asset");

            // 2. Teresa
            var teresa = ScriptableObject.CreateInstance<CharacterDefinition>();
            teresa.characterId = "teresa";
            teresa.characterName = "Commander Teresa";
            teresa.title = "Iron Warden of the Vanguard";
            teresa.role = "Bastion Commander & Military Protector";
            teresa.age = 26;
            teresa.bio = "Thủ lĩnh dày dạn sương gió của tiểu đoàn Thiết Vệ. Teresa tin rằng thế giới bị hủy hoại bởi các pháp sư liều lĩnh, và cô đặt niềm tin duy nhất vào thép gai, chiến thuật và kỷ luật thép.";
            teresa.defaultLocation = "Vanguard Bastion";
            teresa.favoriteGiftItemIds = new List<string> { "starmetal_ingot", "vanguard_aegis", "astral_draught" };
            teresa.startingAffection = 12;
            teresa.startingTrust = 25;
            teresa.startingFamiliarity = 15;
            teresa.startingRespect = 50;
            teresa.startingComfort = 18;
            teresa.startingConflict = 15;
            SaveAsset(teresa, $"{DataRoot}/Characters/Character_Teresa.asset");

            // 3. Vaelen
            var vaelen = ScriptableObject.CreateInstance<CharacterDefinition>();
            vaelen.characterId = "vaelen";
            vaelen.characterName = "Archivist Vaelen";
            vaelen.title = "Curator of the Celestial Vaults";
            vaelen.role = "Elder Astrologer & Keeper of Shards";
            vaelen.age = 42;
            vaelen.bio = "Học giả uyên bác đã dành hơn ba thế kỷ ghi chép lại quỹ đạo của những vì sao tàn lụi. Ông vận hành Thước Đo Tinh Tú và nắm giữ bí mật về các đại cổ trận.";
            vaelen.defaultLocation = "Celestial Archive";
            vaelen.favoriteGiftItemIds = new List<string> { "leyline_shard", "chronos_parchment", "miniature_chronos" };
            vaelen.startingAffection = 20;
            vaelen.startingTrust = 45;
            vaelen.startingFamiliarity = 35;
            vaelen.startingRespect = 60;
            vaelen.startingComfort = 40;
            vaelen.startingConflict = 0;
            SaveAsset(vaelen, $"{DataRoot}/Characters/Character_Vaelen.asset");
        }

        private static void GenerateItems()
        {
            // Seal
            var seal = ScriptableObject.CreateInstance<ItemDefinition>();
            seal.itemId = "luminary_seal";
            seal.nameVi = "Ấn Ký Tinh Tú";
            seal.nameEn = "Crest of the Luminary";
            seal.category = ItemCategory.KeyItem;
            seal.rarity = ItemRarity.Celestial;
            seal.descriptionVi = "Huy hiệu cổ xưa được dệt từ ánh sáng ban mai, khắc gia huy của Edenfall.";
            seal.descriptionEn = "An ancient medallion woven from starlight, etched with the sigil of Edenfall.";
            seal.loreVi = "Bảo vật gia truyền của dòng tộc Solstice, đập cùng nhịp với địa mạch của thánh địa.";
            seal.loreEn = "Passed down through the Solstice bloodline, pulsing with the sanctuary leylines.";
            seal.isUsable = true;
            seal.sanctuaryRestorationBonus = 10f;
            SaveAsset(seal, $"{DataRoot}/Items/Item_LuminarySeal.asset");

            // Lily
            var lily = ScriptableObject.CreateInstance<ItemDefinition>();
            lily.itemId = "celestial_lily";
            lily.nameVi = "Hoa Ly Bình Minh";
            lily.nameEn = "Dawn Celestial Lily";
            lily.category = ItemCategory.Gift;
            lily.rarity = ItemRarity.Rare;
            lily.descriptionVi = "Đóa hoa phát sáng mỏng manh được hái từ ban công Vườn Bình Minh.";
            lily.descriptionEn = "A delicate glowing blossom harvested from the Dawn Garden terrace.";
            lily.loreVi = "Loài hoa yêu thích của Aurelia, chỉ nở rộ dưới ánh sao thuần khiết không vướng hắc ám.";
            lily.loreEn = "Aurelia's favorite flower, blooming only under uncorrupted starlight.";
            lily.targetGiftCharacterId = "aurelia";
            SaveAsset(lily, $"{DataRoot}/Items/Item_CelestialLily.asset");

            // Shard
            var shard = ScriptableObject.CreateInstance<ItemDefinition>();
            shard.itemId = "leyline_shard";
            shard.nameVi = "Mảnh Địa Mạch Bình Minh";
            shard.nameEn = "Dawn Leyline Shard";
            shard.category = ItemCategory.Material;
            shard.rarity = ItemRarity.Rare;
            shard.descriptionVi = "Mảnh pha lê chứa đựng dòng năng lượng thái dương ấm áp.";
            shard.descriptionEn = "A crystal fragment glowing with warm solar energy.";
            shard.loreVi = "Tàn dư kết tinh khi bầu trời Edenfall vỡ vụn vào 300 năm trước.";
            shard.loreEn = "Crystallized resonance from when the celestial firmament shattered 300 years ago.";
            SaveAsset(shard, $"{DataRoot}/Items/Item_LeylineShard.asset");

            // Ingot
            var ingot = ScriptableObject.CreateInstance<ItemDefinition>();
            ingot.itemId = "starmetal_ingot";
            ingot.nameVi = "Thỏi Thép Sao Tiền Tuyến";
            ingot.nameEn = "Vanguard Starmetal Ingot";
            ingot.category = ItemCategory.Gift;
            ingot.rarity = ItemRarity.Rare;
            ingot.descriptionVi = "Khối thép sao tinh luyện dùng để rèn giáp trụ chống lại hắc khí.";
            ingot.descriptionEn = "Dense metal forged to reinforce armor against abyssal miasma.";
            ingot.targetGiftCharacterId = "teresa";
            SaveAsset(ingot, $"{DataRoot}/Items/Item_StarmetalIngot.asset");
        }

        private static void GenerateQuests()
        {
            var qSpire = ScriptableObject.CreateInstance<QuestDefinition>();
            qSpire.questId = "quest_spire";
            qSpire.titleVi = "Đánh Thức Vùng Đất Vỡ Vụn";
            qSpire.titleEn = "The Shattered Awakening";
            qSpire.category = QuestCategory.Main;
            qSpire.summaryVi = "Tòa Tháp Vỡ Vụn phát ra tần số cộng hưởng trầm thấp. Hãy đến tâm điểm di tích và hòa nhịp Lõi Tinh Tú.";
            qSpire.summaryEn = "Dawn Sanctuary's central spire emits a low harmonic frequency. Journey to the heart and attune the Astral Core.";
            qSpire.steps = new List<QuestStep>
            {
                new QuestStep { descriptionVi = "Tiến đến Tòa Tháp Vỡ Vụn tại trung tâm Thánh Địa", descriptionEn = "Approach the Shattered Spire at sanctuary center", targetLocationId = "loc_spire" },
                new QuestStep { descriptionVi = "Dẫn truyền năng lượng cộng hưởng vào Lõi Tinh Tú", descriptionEn = "Channel Luminary Resonance into the Astral Core", targetLocationId = "loc_spire" }
            };
            qSpire.rewardTextVi = "Cộng Hưởng Tinh Tú +15, Mảnh Địa Mạch x2";
            qSpire.rewardTextEn = "Resonance +15, Leyline Shard x2";
            qSpire.rewardResonance = 15f;
            SaveAsset(qSpire, $"{DataRoot}/Quests/Quest_ShatteredAwakening.asset");

            var qGarden = ScriptableObject.CreateInstance<QuestDefinition>();
            qGarden.questId = "quest_garden_solace";
            qGarden.titleVi = "Khắc Khắc Bình Yên Dưới Trăng";
            qGarden.titleEn = "Moonlit Sanctuary Solace";
            qGarden.category = QuestCategory.Character;
            qGarden.summaryVi = "Aurelia thường dạo bước tại Vườn Bình Minh khi hoàng hôn buông xuống. Hãy tìm nàng và chia sẻ tâm sự bên luống hoa ly.";
            qGarden.summaryEn = "Aurelia visits the Dawn Garden when evening arrives. Spend an intimate moment together amidst celestial lilies.";
            qGarden.steps = new List<QuestStep>
            {
                new QuestStep { descriptionVi = "Đến Vườn Bình Minh vào buổi Chiều Tối hoặc Đêm", descriptionEn = "Meet Aurelia at Dawn Garden during Evening or Night", targetLocationId = "loc_garden" },
                new QuestStep { descriptionVi = "Trò chuyện thân mật và khắc ghi kỷ niệm tình cảm", descriptionEn = "Converse intimately with Aurelia and unlock the terrace memory", targetLocationId = "loc_garden" }
            };
            qGarden.rewardTextVi = "Mở Khóa Hoạt Cảnh CG 'Moonlit Sanctuary Solace' & Tăng Sâu Tình Cảm";
            qGarden.rewardTextEn = "Unlock Cinematic CG: 'Moonlit Sanctuary Solace' & Deepen Bond";
            qGarden.rewardResonance = 20f;
            SaveAsset(qGarden, $"{DataRoot}/Quests/Quest_MoonlitSolace.asset");
        }

        private static void GenerateCinematics()
        {
            var cgGarden = ScriptableObject.CreateInstance<CinematicSceneData>();
            cgGarden.sceneId = "cg_aurelia_garden";
            cgGarden.titleVi = "Khắc Khắc Bình Yên Dưới Ánh Trăng";
            cgGarden.titleEn = "Moonlit Sanctuary Solace";
            cgGarden.characterId = "aurelia";
            cgGarden.characterName = "Aurelia";
            cgGarden.descriptionVi = "Khoảnh khắc lãng mạn bên Aurelia tại ban công Vườn Bình Minh dưới làn gió ngát hương hoa ly và bầu trời sao vụn vỡ.";
            cgGarden.descriptionEn = "An intimate evening in Dawn Garden with Aurelia beneath cascading celestial lilies and shattered constellations.";
            cgGarden.minAffection = 20;
            cgGarden.bonusAffection = 15;
            cgGarden.bonusTrust = 15;
            cgGarden.completionMemoryVi = "Đã cùng Aurelia thề ước một lòng bên ban công ngập ánh hoa ly.";
            cgGarden.completionMemoryEn = "Shared an intimate vow beneath the celestial lilies with Aurelia.";

            cgGarden.slides = new List<CinematicSlide>
            {
                new CinematicSlide
                {
                    characterEmotion = CharacterEmotion.Introspective,
                    narrationVi = "Gió đêm nơi Vườn Bình Minh phảng phất hương thơm ngào ngạt của hoa ly tinh tú. Aurelia tựa mình vào lan can cẩm thạch trắng, đôi mắt mơ màng nhìn về những hòn đảo bay lơ lửng giữa vô tận.",
                    narrationEn = "The night air in Dawn Garden is thick with the sweet scent of blooming celestial lilies. Aurelia stands by the white balustrade, gazing toward drifting islands in the void.",
                    dialogueVi = "Mỗi khi nhìn vào khoảng không vô tận ấy... ta lại tự hỏi liệu thiên đường này có thực sự cần được cứu rỗi, hay vạn vật chỉ đang mệt mỏi muốn chìm vào giấc ngủ nghìn thu?",
                    dialogueEn = "Every time I look into the void... I wonder if paradise truly wants to be saved, or if it is merely exhausted.",
                    cameraEffect = CameraEffect.PanUp
                },
                new CinematicSlide
                {
                    characterEmotion = CharacterEmotion.Flustered,
                    narrationVi = "Khi bạn bước tới bên cạnh, những ngón tay nhẹ nhàng chạm vào tà áo lụa mềm mại của nàng. Một làn sóng bối rối khẽ lướt qua ánh mắt Aurelia, nhưng nàng không hề rút tay lại.",
                    narrationEn = "As you step beside her, your fingers lightly brush against the soft silk of her sleeve. A subtle tremor passes through her breath, yet she does not draw away.",
                    dialogueVi = "Ngươi luôn mang đến cảm giác kiên định và tĩnh lặng ấy, Echo. Giữa một thế giới đổ nát và những tiếng thét gào của bạo loạn... ở bên cạnh ngươi khiến lòng ta bình yên lạ thường.",
                    dialogueEn = "You have that quiet resolve, Echo. In a realm full of shattering skies and warlords, your presence is... soothing.",
                    cameraEffect = CameraEffect.SoftGlow
                },
                new CinematicSlide
                {
                    characterEmotion = CharacterEmotion.Radiant,
                    narrationVi = "Aurelia quay hẳn người lại, vương miện hoàng kim trên mái tóc nàng tỏa ra hơi ấm thuần khiết của vầng thái dương. Nàng khẽ vươn bàn tay thon thả, đan chặt những ngón tay vào tay bạn.",
                    narrationEn = "She turns to face you completely. The tiara in her golden hair glows with pure solar warmth. She extends her slender hand, entwining her fingers with yours.",
                    dialogueVi = "Hãy hứa với ta một điều, Echo của ta. Dù cánh cổng Nhật Thực phía trước có mang lại bất cứ tai ương nào... ngươi cũng sẽ mãi ở cạnh ta, có được không?",
                    dialogueEn = "Promise me one thing, my Echo. Whatever happens when the Eclipse Gate opens... you will stay by my side, will you?",
                    cameraEffect = CameraEffect.Radiance
                },
                new CinematicSlide
                {
                    characterEmotion = CharacterEmotion.Intimate,
                    narrationVi = "Hai người đứng sát bên nhau trên ban công lộng gió, những cánh hoa rơi lấp lánh như bụi sao bao phủ lấy cái ôm dịu dàng và lời ước thề vượt qua cả sự sụp đổ của thiên đường.",
                    narrationEn = "The two of you stand close together on the moonlit terrace as glowing petals drift around you, sealed by an embrace that transcends the fall of paradise.",
                    dialogueVi = "Cảm ơn ngươi đã tỉnh giấc, Echo... Lần đầu tiên sau ba trăm năm cô độc, trái tim ta lại thấy ấm áp đến nhường này.",
                    dialogueEn = "Thank you for awakening, my Echo. For the first time in centuries, my heart feels warmth once more.",
                    cameraEffect = CameraEffect.FadeBlack
                }
            };
            SaveAsset(cgGarden, $"{DataRoot}/Cinematics/Cinematic_AureliaGarden.asset");
        }

        private static void GenerateDialogueTrees()
        {
            var tree = ScriptableObject.CreateInstance<DialogueTree>();
            tree.treeId = "tree_aurelia_main";
            tree.characterId = "aurelia";
            tree.initialNodeId = "aur_welcome";

            var node1 = new DialogueNode
            {
                nodeId = "aur_welcome",
                speakerName = "Aurelia",
                emotion = CharacterEmotion.Gentle,
                textVi = "Chào mừng ngươi đã tỉnh giấc tại Thánh Địa Bình Minh, lữ khách. Ta là Aurelia. Rất hiếm ai có thể vượt qua dòng chảy hư không mà vẫn giữ trọn vẹn thần trí. Ngươi có bị thương ở đâu không?",
                textEn = "Welcome to Dawn Sanctuary, outsider. My name is Aurelia. Few awaken with their senses intact after crossing the celestial void. Are you hurt?",
                choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        choiceId = "c1",
                        textVi = "Ta chỉ nhớ được ánh sao và một giai điệu dịu dàng... rồi sau đó nghe thấy giọng nói của nàng.",
                        textEn = "I remember only starlight and a melody... and then your voice.",
                        tone = DialogueTone.Compassionate,
                        affectionDelta = 6,
                        trustDelta = 5,
                        memoryTextVi = "Bộc lộ sự chân thành và yếu mềm ngay khi vừa tỉnh giấc cùng Aurelia.",
                        memoryTextEn = "Spoke with genuine vulnerability upon first waking with Aurelia.",
                        nextNodeId = "aur_reassure"
                    },
                    new DialogueChoice
                    {
                        choiceId = "c2",
                        textVi = "Nàng là ai mà dám chất vấn một Echo? Hãy giải thích cho ta biết điều gì đã xé toạc vùng đất này.",
                        textEn = "Who are you to question an Echo? Tell me what shattered this realm.",
                        tone = DialogueTone.Defiant,
                        conflictDelta = 4,
                        respectDelta = 3,
                        memoryTextVi = "Thể hiện khí chất kiêu hãnh và bất khuất trước Aurelia.",
                        memoryTextEn = "Exhibited fiery pride and independence before Aurelia.",
                        nextNodeId = "aur_defiant"
                    },
                    new DialogueChoice
                    {
                        choiceId = "c3",
                        textVi = "Ta muốn tìm hiểu về Thánh Địa này và Cánh Cổng Nhật Thực ở phía nam.",
                        textEn = "I need to understand this sanctuary and the Eclipse Gate to the south.",
                        tone = DialogueTone.Inquisitive,
                        trustDelta = 4,
                        respectDelta = 4,
                        nextNodeId = "aur_explain"
                    }
                }
            };

            var nodeIntimate = new DialogueNode
            {
                nodeId = "aur_intimate",
                speakerName = "Aurelia",
                emotion = CharacterEmotion.Radiant,
                textVi = "Mỗi khi ngươi nhìn ta bằng ánh mắt ấy, Echo... ta lại cảm thấy Thiên Đường Vỡ Vụn này không hề mất đi, mà đang chờ chúng ta cùng nhau tái thiết. Tim ta đập rộn ràng mỗi khi ngươi bước vào Vườn Bình Minh.",
                textEn = "Whenever you look at me like that, Echo... I feel this Shattered Paradise is waiting for us to rebuild together. My heart races whenever you enter Dawn Garden.",
                choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        choiceId = "c_romance",
                        textVi = "Vậy hãy cùng ta tái sinh lại Edenfall, Aurelia. Đến gần ta hơn nữa đi.",
                        textEn = "Then let us rebuild Edenfall together, Aurelia. Come closer.",
                        tone = DialogueTone.Romantic,
                        affectionDelta = 8,
                        comfortDelta = 10,
                        triggerCinematicId = "cg_aurelia_garden",
                        memoryTextVi = "Trao nhau lời thề ước ngọt ngào dưới vòm hoa tinh tú.",
                        memoryTextEn = "Shared a sweet, tender vow beneath the starlight blossoms."
                    },
                    new DialogueChoice
                    {
                        choiceId = "c_flirt",
                        textVi = "Nàng còn rực rỡ và cuốn hút hơn cả triệu vì sao trên vòm trời kia.",
                        textEn = "You are far more captivating than the millions of stars above.",
                        tone = DialogueTone.Flirtatious,
                        affectionDelta = 7,
                        comfortDelta = 5
                    }
                }
            };

            var nodeReassure = new DialogueNode
            {
                nodeId = "aur_reassure",
                speakerName = "Aurelia",
                emotion = CharacterEmotion.Smiling,
                textVi = "Ngươi thật có tài ăn nói... Giọng điệu của ngươi như ánh nắng sưởi ấm những tảng băng nghìn năm trong thánh điện. Hãy nhận đóa Hoa Ly Bình Minh này, nó sẽ bảo hộ cho linh hồn ngươi.",
                textEn = "You have a gentle way with words... Your tone warms the ancient ice in this sanctuary. Take this Dawn Lily, it will protect your spirit.",
                choices = new List<DialogueChoice>()
            };

            var nodeDefiant = new DialogueNode
            {
                nodeId = "aur_defiant",
                speakerName = "Aurelia",
                emotion = CharacterEmotion.Serious,
                textVi = "Khí khái đáng khen, nhưng lòng kiêu hãnh mù quáng sẽ chỉ dẫn ngươi vào nanh vuốt của Hắc Khí Vực Thẳm. Nếu ngươi muốn sống sót, hãy học cách tin tưởng đồng minh.",
                textEn = "A fierce spirit, but blind pride will only lead you into the jaws of the Miasma. If you wish to survive, learn to trust allies.",
                choices = new List<DialogueChoice>()
            };

            var nodeExplain = new DialogueNode
            {
                nodeId = "aur_explain",
                speakerName = "Aurelia",
                emotion = CharacterEmotion.Introspective,
                textVi = "Nơi này là tàn tích cuối cùng của Vầng Thái Dương. Cánh Cổng Nhật Thực phía nam đang rạn nứt từng ngày. Nếu chúng ta không kích hoạt lại Lõi Tinh Tú tại Tòa Tháp Vỡ Vụn, bóng tối sẽ nuốt chửng tất cả.",
                textEn = "This place is the last remnant of the Solstice Sun. The Eclipse Gate fractures daily. If we do not attune the Astral Core, darkness will consume all.",
                choices = new List<DialogueChoice>()
            };

            tree.nodes = new List<DialogueNode> { node1, nodeIntimate, nodeReassure, nodeDefiant, nodeExplain };
            SaveAsset(tree, $"{DataRoot}/Dialogue/DialogueTree_Aurelia.asset");
        }

        private static void GenerateSchedules()
        {
            var sched = ScriptableObject.CreateInstance<NPCSchedule>();
            sched.entries = new List<ScheduleEntry>
            {
                new ScheduleEntry
                {
                    timeOfDay = TimeOfDay.Morning,
                    locationName = "Dawn Garden",
                    targetPosition = new Vector2(-4f, 3f),
                    activityDescription = "Chăm sóc hoa ly tinh tú và hứng sương mai ban sớm",
                    poseAnimation = "kneeling"
                },
                new ScheduleEntry
                {
                    timeOfDay = TimeOfDay.Afternoon,
                    locationName = "Celestial Altar",
                    targetPosition = new Vector2(0f, 0f),
                    activityDescription = "Kiểm tra các vết rạn trên Lõi Tinh Tú và điều hòa tần số",
                    poseAnimation = "standing"
                },
                new ScheduleEntry
                {
                    timeOfDay = TimeOfDay.Evening,
                    locationName = "Solstice Balcony",
                    targetPosition = new Vector2(5f, -2f),
                    activityDescription = "Đứng bên ban công ngắm nhìn quần đảo bay trôi dạt lúc hoàng hôn",
                    poseAnimation = "pensive"
                },
                new ScheduleEntry
                {
                    timeOfDay = TimeOfDay.Night,
                    locationName = "Aurelia's Private Chamber",
                    targetPosition = new Vector2(0f, 2f),
                    activityDescription = "Nghỉ ngơi bên trong phòng riêng dưới làn gió mát và ánh trăng thanh",
                    poseAnimation = "resting"
                }
            };
            SaveAsset(sched, $"{DataRoot}/Schedules/Schedule_Aurelia.asset");
        }

        private static void GenerateLocations()
        {
            string artEnv = "Assets/_Edenfall/Art/Environment";

            // 1. Dawn Garden
            var locGarden = ScriptableObject.CreateInstance<LocationDefinition>();
            locGarden.locationId = "loc_dawn_garden";
            locGarden.nameVi = "Vườn Bình Minh";
            locGarden.nameEn = "Dawn Garden";
            locGarden.descriptionVi = "Khu vườn thánh địa tĩnh lặng tràn ngập hương hoa ly tinh tú và làn gió sương mai.";
            locGarden.descriptionEn = "A serene sanctuary terrace perfumed with celestial lilies under soft morning light.";
            locGarden.locationType = LocationType.Garden;
            locGarden.backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{artEnv}/Sanctuary_MarblePlatform.png");
            locGarden.distantSpireSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{artEnv}/Spire_SolsticeDistant.png");
            locGarden.isUnlockedByDefault = true;
            locGarden.hotspots = new List<LocationHotspot>
            {
                new LocationHotspot { hotspotId = "spot_garden_talk", titleVi = "Trò chuyện cùng Aurelia", titleEn = "Talk with Aurelia", actionType = "talk" },
                new LocationHotspot { hotspotId = "spot_garden_lily", titleVi = "Ngắm hoa ly tinh tú", titleEn = "Admire Celestial Lilies", actionType = "inspect" }
            };
            SaveAsset(locGarden, $"{DataRoot}/Locations/Location_DawnGarden.asset");

            // 2. Celestial Altar
            var locAltar = ScriptableObject.CreateInstance<LocationDefinition>();
            locAltar.locationId = "loc_celestial_altar";
            locAltar.nameVi = "Đài Tế Tinh Tú";
            locAltar.nameEn = "Celestial Altar";
            locAltar.descriptionVi = "Bệ đá cẩm thạch thiêng liêng nơi mảnh lõi mặt trời rực cháy vĩnh hằng.";
            locAltar.descriptionEn = "A consecrated marble altar housing the pulsating celestial sun shard.";
            locAltar.locationType = LocationType.Altar;
            locAltar.backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Edenfall/Art/Props/Prop_CelestialAltar.png");
            locAltar.isUnlockedByDefault = true;
            locAltar.hotspots = new List<LocationHotspot>
            {
                new LocationHotspot { hotspotId = "spot_altar_resonate", titleVi = "Cảm nhận nhịp đập địa mạch", titleEn = "Sense Leyline Pulse", actionType = "inspect" }
            };
            SaveAsset(locAltar, $"{DataRoot}/Locations/Location_CelestialAltar.asset");

            // 3. Solstice Balcony
            var locBalcony = ScriptableObject.CreateInstance<LocationDefinition>();
            locBalcony.locationId = "loc_solstice_balcony";
            locBalcony.nameVi = "Ban Công Hạ Chí";
            locBalcony.nameEn = "Solstice Balcony";
            locBalcony.descriptionVi = "Ban công trên cao nhìn ra tòa tháp tinh tú vỡ vụn và những hòn đảo trôi lơ lửng.";
            locBalcony.descriptionEn = "A high terrace overlooking the Shattered Spire and drifting crystalline islands.";
            locBalcony.locationType = LocationType.Balcony;
            locBalcony.backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{artEnv}/Spire_SolsticeDistant.png");
            locBalcony.isUnlockedByDefault = true;
            locBalcony.hotspots = new List<LocationHotspot>
            {
                new LocationHotspot { hotspotId = "spot_balcony_vow", titleVi = "Khắc ghi lời thề Tinh Tú", titleEn = "Recite Solstice Vow", actionType = "event" }
            };
            SaveAsset(locBalcony, $"{DataRoot}/Locations/Location_SolsticeBalcony.asset");

            // 4. Aurelia's Chamber
            var locChamber = ScriptableObject.CreateInstance<LocationDefinition>();
            locChamber.locationId = "loc_aurelia_chamber";
            locChamber.nameVi = "Khu Nghỉ Dưỡng Nàng Aurelia";
            locChamber.nameEn = "Aurelia's Private Chamber";
            locChamber.descriptionVi = "Phòng nghỉ trang nhã với giường lụa thêu chỉ vàng, hoa tử đằng buông rủ và ban công đón nắng sớm.";
            locChamber.descriptionEn = "An intimate sanctuary bedchamber draped in silk and wisteria overlooking the celestial sky.";
            locChamber.locationType = LocationType.PrivateChamber;
            locChamber.backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{artEnv}/Chamber_SolsticeHeir.png");
            locChamber.isUnlockedByDefault = false;
            locChamber.requiredAffectionToUnlock = 35;
            locChamber.unlockRequirementDescriptionVi = "Đạt 35 Điểm Tình Cảm cùng Aurelia";
            locChamber.unlockRequirementDescriptionEn = "Reach 35 Affection with Aurelia";
            locChamber.hotspots = new List<LocationHotspot>
            {
                new LocationHotspot { hotspotId = "spot_chamber_intimacy", titleVi = "Tâm sự riêng tư buổi sớm", titleEn = "Private Morning Intimacy", actionType = "event" }
            };
            SaveAsset(locChamber, $"{DataRoot}/Locations/Location_AureliaChamber.asset");
        }

        private static void GenerateMatureEvents()
        {
            var evtIntimacy = ScriptableObject.CreateInstance<MatureEventDefinition>();
            evtIntimacy.eventId = "evt_aurelia_morning_intimacy";
            evtIntimacy.titleVi = "Khoảnh Khắc Thân Mật Bên Nàng Aurelia";
            evtIntimacy.titleEn = "Morning Intimacy with Aurelia";
            evtIntimacy.characterId = "aurelia";
            evtIntimacy.summaryVi = "Khoảnh khắc ngọt ngào bên Aurelia tại phòng nghỉ riêng tư dưới ánh nắng sương mai rực rỡ.";
            evtIntimacy.summaryEn = "A sweet, intimate morning shared with Aurelia in her private sanctuary chamber.";
            evtIntimacy.minAffection = 35;
            evtIntimacy.minTrust = 30;
            evtIntimacy.minComfort = 25;
            evtIntimacy.requiredLocationId = "loc_aurelia_chamber";
            evtIntimacy.requiredTimeOfDay = TimeOfDay.Morning;
            evtIntimacy.anyTimeOfDay = true;
            evtIntimacy.mainCGSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Edenfall/Art/Cinematics/CG_ChamberIntimacy.png");

            evtIntimacy.slides = new List<MatureEventSlide>
            {
                new MatureEventSlide
                {
                    characterEmotion = CharacterEmotion.Smiling,
                    narrationVi = "Ánh nắng ban mai rọi qua vòm cột đá cẩm thạch trắng, nhuộm vàng những cánh hoa tử đằng đang đung đưa trong gió thoảng. Aurelia tựa mình trên chiếc giường lụa êm ái, ngước mắt nhìn bạn với nụ cười dịu dàng.",
                    narrationEn = "Gentle morning sunbeams filter through classical marble archways, illuminating purple wisteria petals. Aurelia rests softly upon the silk bed, gazing up at you with an intimate, radiant smile.",
                    dialogueVi = "Chào buổi sáng, người thương của ta... Ngươi đã thức giấc rồi sao? Ta cứ ngỡ ngươi vẫn còn đang chìm trong giấc mơ về vùng đất vỡ vụn này.",
                    dialogueEn = "Good morning, my sweet... Are you finally awake? I thought you were still lost in dreams of this shattered world.",
                    cameraEffect = CameraEffect.ZoomIn
                },
                new MatureEventSlide
                {
                    characterEmotion = CharacterEmotion.Flustered,
                    narrationVi = "Đôi má nàng phơn phớt ửng hồng. Nàng khẽ đưa bàn tay thon thả về phía bạn, từng đầu ngón tay lấp lánh bụi tinh tú ấm áp.",
                    narrationEn = "A delicate rose blush warms her cheeks. She extends a graceful hand toward you, her fingertips glowing with warm celestial light.",
                    dialogueVi = "Hãy lại gần đây... Chỉ cần có ngươi kề bên, gánh nặng của dòng dõi Hạ Chí dường như tan biến vào hư vô.",
                    dialogueEn = "Come closer... As long as you are by my side, the weight of the Solstice bloodline feels lighter than air.",
                    cameraEffect = CameraEffect.Radiance
                },
                new MatureEventSlide
                {
                    characterEmotion = CharacterEmotion.Intimate,
                    isFadeToBlack = true,
                    narrationVi = "Căn phòng ngập tràn hơi thở ấm áp và hương hoa ly thanh tao. Khoảnh khắc hai trái tim hòa chung nhịp đập dưới ánh mặt trời rạng đông.",
                    narrationEn = "The chamber fills with gentle warmth and the delicate fragrance of lilies. A private vow shared between two souls beneath the morning sun.",
                    dialogueVi = "Ta yêu ngươi... Hãy mãi là vầng thái dương của riêng ta, Echo nhé.",
                    dialogueEn = "I love you... Please remain my own personal sun forever, Echo.",
                    cameraEffect = CameraEffect.FadeBlack
                }
            };

            SaveAsset(evtIntimacy, $"{DataRoot}/Events/Event_MorningIntimacy.asset");
        }

        private static void SaveAsset(Object asset, string path)
        {
            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
#endif
