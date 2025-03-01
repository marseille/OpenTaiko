﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FDK;

namespace TJAPlayer3
{
    internal class CLang_fr : ILang
    {
        string ILang.GetString(int idx)
        {
            if (!dictionnary.ContainsKey(idx))
                return "[!] Index non trouvé dans le dictionnaire";

            return dictionnary[idx];
        }


        private static readonly Dictionary<int, string> dictionnary = new Dictionary<int, string>
        {
            [0] = "Changer la langue affichée\ndans les menus et en jeu.",
            [1] = "Langue du système",
            [2] = "<< Retour au menu",
            [3] = "Retour au menu principal.",
            [4] = "Recharger les sons",
            [5] = "Met à jour et récupère les\nmodifications effectuées sur\nla liste de sons.",
            [6] = "Nombre de joueurs",
            [7] = "Change le nombre de joueurs en jeu：\nEn le mettant à 2, il est possible de\njouer à 2 en mode écran scindé.\nDisponible seulement pour le mode partie \nrapide.",
            [8] = "Mort subite",
            [9] = "Mode mort subite :\nSi 1 ou plus, spécifiez le nombre de \nnotes ratées maximales autorisées avant \nde perdre la partie.\nSi 0 le mode mort subite est désactivé.",
            [10] = "Vitesse générale",
            [11] = "Change le coefficient multiplicateur de \nla vitesse générale de la musique en jeu." +
                "Par exemple, vous pouvez la diviser par \n" +
                "2 en l'établissant à 0.500 \n" +
                "afin de vous entraîner plus serainement.\n" +
                "\n" +
                "Note: Cette option change aussi le ton de la musique.\n" +
                "Si TimeStretch=ON, il peut y avoir du\n" +
                "lag si la vitesse générale est inférieure à x0.900.",
            [12] = "Niveau de l'IA",
            [13] = "Determine les performances de l'IA.\n" +
                "Si 0, l'IA est désactivée.\n" +
                "Si au moins 1, le J2 est joué par l'IA.\n" +
                "Non compatible avec le mode AUTO J2.",
            [14] = "Décalage général",
            [15] = "Modifie la valeur OFFSET\nlue pour tout les sons.\n" +
                "Définit entre -99 to 99ms.\n" +
                "Une valeur négative peut réduire \nles latences d'entrées.\n\n" +
                "Note: Cette option prend effet\n" +
                "     après le rechargement des sons.",
            [16] = "Disposition des blocs",
            [17] = "Cette option détermine l'ordonnancement \ndes blocs dans le menu de selection \n des musiques en mode partie rapide.\n" +
                "0 : Standard (Diagonale haut-bas)\n" +
                "1 : Vertical\n" +
                "2 : Diagonale bas-haut\n" +
                "3 : Demi-cercle orienté à droite\n" +
                "4 : Demi-cercle orienté à gauche",
            [18] = "Non utilisé, ancienne option de DTXMania.",
            [19] = "Choisir entre le mode plein \nécran et fenêtre.",
            [20] = "Non utilisé, ancienne option de DTXMania.",
            [21] = "Parcourir les sous-dossiers \nrécursivement lors de l'utilisation \ndu bouton 'Surprend moi !'.",
            [22] = "Activer ou non la synchronisation verticale.\nLimite les FPS à 60,\nrendant le défilement fluide\nmais augmentant la latence des entrées.\nSi désactivé, il n'y a pas de limite\naux FPS donc moins de latences,\nmais le défilement des notes peut être sacadé.",
            [23] = "Activer ou non les vidéos en jeu.\nSi activé et qu'une musique ne contient\npas de vidéo, le décor est un fond noir.",
            [24] = "Activer ou non les animations de décor.",
            [25] = "Temps pris avant la prévisualisation\nde la musique dans le menu.\nRéduire cette valeur\npeut faire que la prévisualisation\nsoit lancée avant l'animation du menu.",
            [26] = "Non utilisé, ancienne option de DTXMania.",
            [27] = "Activer ou non les informations \nde déboggage en jeu.",
            [28] = "Contrôle l'opacité des animations de décor.",
            [29] = "Activer ou non la musique en jeu.",
            [30] = "Activer ou non la sauvegarde sous un fichier score.ini.",
            [31] = "Non utilisé, ancienne option de DTXMania.",
            [32] = "Non utilisé, ancienne option de DTXMania.",
            [33] = "Active l'option SONGVOL dans les fichiers .tja.\nPermet de faire varier le volume des notes.",
            [34] = "Ajuste le volume des notes et effets sonores.\nSi 0 aucun son ne sera joué lors d'une frappe.\nVous devez redemarrer le jeu pour que\ncette option prenne effet.",
            [35] = "Ajuste le volume des voix.\nVous devez redemarrer le jeu pour que\ncette option prenne effet.",
            [36] = "Ajuste le volume de la musique.\nVous devez redemarrer le jeu pour que\ncette option prenne effet.",
            [37] = "Reduit le volume en appuyant sur [\n et l'augmente en appuyant sur ].\nDéfinit l'amplitude modifiée par appui.\nEntre 1 et 20.",
            [38] = "Temps de delai avant le début de la musique en jeu.\nReduire cette valeur peut faire que\nla musique soit jouée trop tôt.",
            [39] = "Permet la prise automatique de capture d'écran\nà la fin d'une partie.\nSeulement déclanché en cas de meilleur score,\nqui peut ne pas refleter parfaitement\nla qualité de celui-ci.",
            [40] = "Active ou non le partage d'informations\nde jeu avec Discord.",
            [41] = "Permet d'éviter la perte de frappes en\ncas de chute de FPS.\nSi désactivé, des frappes peuvent être\nperdues mais seront plus souvent mises\nen attendes en cas de freeze.",
            [42] = "Génère un fichier TJAPlayer3.log file à la fermeture\ndu jeu.\nPermet l'évaluation des performances et\nfacilite l'identification des erreurs.",
            [43] = "ASIO:\n- Fonctionne seulement avec les appareils compatibles.\n- Latence d'entrée les plus faibles\nWasapi:\n- Latence d'entrée généralement faible\n- Bloque les sons exterieurs à OpenTaiko\nDirect Sound:\n- Permet les sons exterieurs à OpenTaiko\n- Latence d'entrée élevée\n",
            [44] = "Change la taille du buffer WASAPI.\nA définir au plus bas possible\nsans causer de problèmes de son\ncomme des freezes ou un timing incorrect.\nLe definir à 0 pour une estimation automatique,\nou chercher la valeur la plus appropriée\nen en essayant plusieurs.",
            [45] = "Choisir une inderface pour ASIO.",
            [46] = "Permet un défilement plus fluide des notes,\nmais peut augmenter les latences sonores.\nLe désactiver peut rendre le défilement\nplus sacadé,\nmais assure la présence d'aucun lag sonore.\n" +
                "Seulement disponible avec\n" +
                "WASAPI ou ASIO.\n",
            [47] = "Afficher les personnages en jeu.\n",
            [48] = "Afficher les danceurs en jeu.\n",
            [49] = "Afficher le mob en jeu.\n",
            [50] = "Afficher les coureurs en jeu.\n",
            [51] = "Afficher le footer en jeu.\n",
            [52] = "Activer ou non le rendu des images avant le chargement des musiques.\n",
            [53] = "Afficher les PuchiCharas en jeu.\n",
            [54] = "Choisir le skin utiliser dans le dossier System.",
            [55] = "Menu pour l'assignation des \ntouches système.",
            [56] = "Joueur 1 Auto",
            [57] = "Activer le mode automatique pour\nle joueur 1.\nActivable en appuyant sur F3 dans \nle menu de selection des musiques.",
            [58] = "Joueur 2 Auto",
            [59] = "Activer le mode automatique pour\nle joueur 2.\nActivable en appuyant sur F4 dans \nle menu de selection des musiques.",
            [60] = "Vitesse des rolls",
            [61] = "Nombre de frappes par seconde lors\ndes rolls en mode automatique.\nDésactivé si 0, au maximum une\nfrappe par image.",
            [62] = "Vitesse de défilement",
            [63] = "Changer la vitesse de défilement\ndes notes.\n" +
                "De x0.1 à x200.0.\n",
            [64] = "Mort subite",
            [65] = "Mode mort subite :\nSi 1 ou plus, spécifiez le nombre de \nnotes ratées maximales autorisées avant \nde perdre la partie.\nSi 0 le mode mort subite est désactivé.",
            [66] = "Mode shuffle",
            [67] = "Les notes sont changées de manière aléatoire.\nTaux d'aléatoire allant de 'Part' à 'Hyper'.",
            [68] = "Notes cachées",
            [69] = "DORON: Les notes sont cachées.\n" +
                "STEALTH: Le notes et le texte en \ndessous sont cachés.",
            [70] = "Pas d'informations",
            [71] = "Activer ou non l'affichage \ndes informations des musiques.",
            [72] = "Mode Punitif",
            [73] = "Remplace les OK par des MAUVAIS.",
            [74] = "Notes vérouillées",
            [75] = "Si activé, taper le tambour entre\ndeux notes ajoute un MAUVAIS.",
            [76] = "Combo affiché minimum",
            [77] = "Définit le combo minimum affiché.\n" +
                "Entre 1 et 99999.",
            [78] = "Déplacer la zone de jugement",
            [79] = "Déplace la zone de jugement des notes.\nUtile en cas de latence lors des entrées.",
            [80] = "Difficulté par défaut",
            [81] = "Change la difficulté par défaut lors de\nl'écran de selection des musiques.",
            [82] = "Méthode de Scoring",
            [83] = "Choisir la formule de calcul du score.\n" +
                    "TYPE-A: Gen-1\n" +
                    "TYPE-B: Gen-2\n" +
                    "TYPE-C: Gen-3\n",
            [84] = "Utilise la methode Gen-4 de\ncalcul du score.",
            [85] = "Guide des branches",
            [86] = "Affiche un guide numérique\nafin de voir quelle branche sera choisie.\nDésactivé en mode automatique.",
            [87] = "Set d'animation des branches",
            [88] = "Change le set d'animation\nlors de la présence d'une branche.\n" +
                    "TYPE-A: Gen-2\n" +
                    "TYPE-B: Gen-3\n",
            [89] = "Mode survie",
            [90] = "Non fonctionnel.",
            [91] = "Double frappes",
            [92] = "Impose de frapper les deux côtés\ndu tambour afin d'obtenir le bonus x2 points\npour les grandes notes.",
            [93] = "Resultats en direct",
            [94] = "Affiche le nombre de BON/OK/MAUVAIS\nen direct sur le bas de l'écran.\n" +
                "(Seulement en mode 1 joueur)",
            [95] = "Touches en jeu",
            [96] = "Menu pour l'assignation des \ntouches en jeu.",
            [97] = "Capture d'écran",
            [98] = "Assigner la touche pour le \ncapture d'écran.",
            [99] = "Rouge gauche",
            [10000] = "Assignation de la touche \nRouge gauche pour le tambour.",
            [10001] = "Rouge droit",
            [10002] = "Assignation de la touche \nRouge droit pour le tambour.",
            [10003] = "Bleu gauche",
            [10004] = "Assignation de la touche \nBleu gauche pour le tambour.",
            [10005] = "Bleu droit",
            [10006] = "Assignation de la touche \nBleu droit pour le tambour.",
            [10007] = "Rouge gauche 2P",
            [10008] = "Assignation de la touche \nRouge gauche pour le tambour. \n(2P)",
            [10009] = "Rouge droit 2P",
            [10010] = "Assignation de la touche \nRouge droit pour le tambour. \n(2P)",
            [10011] = "Bleu gauche 2P",
            [10012] = "Assignation de la touche \nBleu gauche pour le tambour. \n(2P)",
            [10013] = "Bleu droit 2P",
            [10014] = "Assignation de la touche \nBleu droit pour le tambour. \n(2P)",
            [10018] = "Mode Time Stretch",
            [10019] = "Plein écran",
            [10020] = "Mode Game Over",
            [10021] = "Sous-dossiers pour la séléction aléatoire",
            [10022] = "Mode VSync",
            [10023] = "Activer les vidéos de fond",
            [10024] = "Afficher le décor",
            [10025] = "Délai avant la prévisualisation de la musique",
            [10026] = "Délai avant l'image",
            [10027] = "Mode Debug",
            [10028] = "Opacité du décor",
            [10029] = "Activer la musique en jeu",
            [10030] = "Sauvegarder les scores",
            [10031] = "Apply Loudness Metadata (Non utilisé)",
            [10032] = "Target Loudness (Non utilisé)",
            [10033] = "Activer l'option SONGVOL",
            [10034] = "Volume des effets sonores",
            [10035] = "Volume des voix",
            [10036] = "Volume de la musique",
            [10037] = "Volume du clavier",
            [10038] = "Délai avant la musique",
            [10039] = "Capture d'écran automatique",
            [10040] = "Rich Presence Discord",
            [10041] = "Bufferisation des entrées",
            [10042] = "Journalisation",
            [10043] = "Interface sonore",
            [10044] = "Taille du buffer Wasapi",
            [10045] = "Interface Asio",
            [10046] = "Utiliser le timer de l'OS",
            [10047] = "Afficher les personnages",
            [10048] = "Afficher les danceurs",
            [10049] = "Afficher le mob",
            [10050] = "Afficher les coureurs",
            [10051] = "Afficher le footer",
            [10052] = "Affichage rapide",
            [10053] = "Afficher les PuchiCharas",
            [10054] = "Skin actuel",
            [10055] = "Touches systeme",
            [10056] = "Masquer Dan/Tour",
            [10057] = "Masque les sons de type Dan ou\nTour dans le menu Partie rapide.\n" +
            "Note: Cette option prend effet\n" +
                "     après le rechargement des sons.",
            [10058] = "Volume de la prévisualisation de la musique",
            [10059] = "Ajuste le volume de la prévisualisation de la musique.\nVous devez redemarrer le jeu pour que\ncette option prenne effet.",
            [10060] = "Clap",
            [10061] = "Assignation de la touche \nClap pour les bongos.",
            [10062] = "Clap 2P",
            [10063] = "Assignation de la touche \nClap pour les bongos. \n(2P)",

            [10064] = "Decide",
            [10065] = "Touche de selection \ndans les menus.",
            [10066] = "Cancel",
            [10067] = "Touche d'annulation \ndans les menus.",
            [10068] = "LeftChange",
            [10069] = "Touche de navigation (gauche) \ndans les menus.",
            [10070] = "RightChange",
            [10071] = "Touche de navigation (droite) \ndans les menus.",

            [10084] = "Mode Shin'uchi",
            [10085] = "Options système",
            [10086] = "Options de jeu",
            [10087] = "Quitter",
            [10091] = "Settings for an overall systems.",
            [10092] = "Settings to play the drums.",
            [10093] = "Save the settings and exit from CONFIGURATION menu.",


            [100] = "Partie rapide",
            [101] = "Défis du Dojo",
            [102] = "Tours rhytmiques",
            [103] = "Magasin",
            [104] = "Aventure",
            [105] = "Ma Pièce",
            [106] = "Paramètres",
            [107] = "Quitter le jeu",
            [108] = "Salon 'l'En-Ligne'",
            [109] = "Encyclopedie",
            [110] = "Mode contre l'IA",
            [111] = "Statistiques",
            [112] = "Editeur de partition",
            [113] = "Boîte à outils",

            [150] = "Jouez vos sons favoris\nà votre propre rhythme !",
            [151] = "Jouez plusieurs sons à la suite\nen suivant des règles exigentes\ndans le but de reussir le défi !",
            [152] = "Jouez de longs sons avec un\nnombre de vies limité et\natteignez le sommet de la tour !",
            [153] = "Achetez de nouveaux sons ou personnages\ngrâce aux médailles acquises en jeu !",
            [154] = "Surmontez une multitude d'obstables\nafin de découvrir du nouveau contenu\net de nouveaux horizons !",
            [155] = "Changez votre personnage\nou les informations de votre\nplaque nominative !",
            [156] = "Changez votre style de jeu\n ou les paramètres généraux !",
            [157] = "Quitter le jeu.\nÀ bientôt !",
            [158] = "Telechargez de nouvelles\npartitions et du nouveau\ncontenu depuis internet !",
            [159] = "Apprenez à propos des\nnouvelles fonctionalitées et\ncomment ajouter du contenu!",
            [160] = "Combattez une IA puissante à\ntravers plusieurs sections et\ndécrochez la victoire !",
            [161] = "Suivez votre progression en \ndirect !",
            [162] = "Créez vos propres partitions\n.tja avec vos sons favoris !",
            [163] = "Utilisez divers outils\nproposés afin de faciliter\nl'ajout de contenu !",

            [200] = "Retour",
            [201] = "Sons joués récemment",
            [202] = "Rejouez les sons joués précedement !",
            [203] = "Surprend moi !",

            [300] = "Jetons obtenus !",
            [301] = "Personnage obtenu !",
            [302] = "Petit personnage obtenu !",
            [303] = "Titre obtenu !",
            [304] = "Notification",
            [305] = "Confirmation",
            [306] = "Jetons",
            [307] = "Total",

            [400] = "Retour au menu principal",
            [401] = "Retour",
            [402] = "Télécharger du contenu",
            [403] = "Choisir un CDN",
            [404] = "Télécharger des Sons",
            [405] = "Télécharger des Personnages",
            [406] = "Télécharger des Puchicharas",
            [407] = "Multijoueur en ligne",

            [500] = "Timing",
            [501] = "Laxiste",
            [502] = "Permissif",
            [503] = "Normal",
            [504] = "Strict",
            [505] = "Rigoureux",
            [510] = "Multiplicateur de score : ",
            [511] = "Multiplicateur de pièces : ",
            [512] = "Type de jeu",
            [513] = "Taiko",
            [514] = "Konga",
            [515] = "Extras",
            [516] = "Avalanche",
            [517] = "Démineur",

            [1000] = "Étage atteint",
            [1001] = "",
            [1002] = "P",
            [1003] = "Score",

            [1010] = "Jauge d'âme",
            [1011] = "Nombre de Bon",
            [1012] = "Nombre de Ok",
            [1013] = "Nombre de Mauvais",
            [1014] = "Score",
            [1015] = "Frappes successives",
            [1016] = "Nombre de frappes",
            [1017] = "Combo",
            [1018] = "Précision",
            [1019] = "Nombre d'ADLIB",
            [1020] = "Bombes activées",

            [1030] = "Retour",
            [1031] = "Petit Personnage",
            [1032] = "Personnage",
            [1033] = "Titre Dan",
            [1034] = "Titre",

            [1040] = "Facile",
            [1041] = "Normal",
            [1042] = "Difficile",
            [1043] = "Extrême",
            [1044] = "Extra",
            [1045] = "Extrême / Extra",

            [90000] = "[ERREUR] Condition invalide",
            [90001] = "L'article n'est disponible que dans le Magasin.",
            [90002] = "Prix en jetons : ",
            [90003] = "Article acheté !",
            [90004] = "Nombre de jetons insuffisant !",
            [90005] = "La condition suivante : ",

            [900] = "Reprendre",
            [901] = "Recommencer",
            [902] = "Quitter",

            [910] = "IA",
            [911] = "Deus-Ex-Machina",

            [9000] = "Non",
            [9001] = "Oui",
            [9002] = "Aucun",
            [9003] = "Hasardeux",
            [9004] = "Chaotique",
            [9006] = "Mode entraînement",
            [9007] = "-",
            [9008] = "Vitesse",
            [9009] = "Discret",
            [9010] = "Inverse",
            [9011] = "Hasard",
            [9012] = "Mode de jeu",
            [9013] = "Auto",
            [9014] = "Voix",
            [9015] = "Instrument",
            [9016] = "Furtif",
            [9017] = "Protegé",
            [9018] = "Punitif",

            [9100] = "Rechercher (Difficulté)",
            [9101] = "Difficulté",
            [9102] = "Niveau",
        };
    }
}