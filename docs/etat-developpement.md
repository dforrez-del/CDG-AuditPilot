# État vérifié — reprise du 22 octobre 2026

Base examinée : `main` au commit d8ea284 et branche `codex/socle-clients-missions` au commit d749b52. Ces branches avaient divergé : la première utilisait JSON et six onglets ; la seconde apportait EF Core/SQLite, les entretiens détaillés et les flux. L’interface de main et ses fonctions ont été conservées et les modules de la branche précédente réintégrés, sans repartir de zéro.

## Vérifications locales

- SDK .NET 10.0.402 : compilation de la solution en Release, 0 erreur, 0 avertissement.
- 34 tests xUnit réussis : persistance, migration, imports, API simulée, flux, preuves, liens inter-missions, concurrence, chiffrement/restauration, DOCX/XLSX/PDF.
- NuGet signalait Pkcs transitif : épinglé à 10.0.13, version corrigée selon GHSA-846f-f764-76mx et GHSA-2fmv-rv2c-59x7.
- Ces résultats Linux ne prouvent pas le fonctionnement visuel Windows.

## Ajouts intégrés

Migration du JSON historique sans suppression de l’original ; conservation des dossiers SQLite existants ; sauvegardes transactionnelles et contrôle de révision ; bibliothèque de questionnaires JSON à versions immuables ; entretiens détaillés et autosauvegarde ; preuves copiées et vérifiées SHA-256 ; rattachement aux constats et revue humaine ; édition des actions et missions ; clés API DPAPI ; sauvegardes AES-GCM manuelles et automatiques ; restauration en dossier intermédiaire avec manifeste ; exports DOCX, PDF, XLSX ; diagrammes PNG/SVG avec inclusion des PNG conservés dans les rapports.

## État initial de livraison (historique)

La CI Windows doit encore être exécutée et son résultat lu. Elle compile, teste, lance un parcours WPF automatisé, produit des captures et rapports, construit un installateur Inno Setup x64 puis teste son installation silencieuse. Aucun installateur n’est déclaré validé avant ces contrôles.

## Écarts identifiés à la reprise (historique)

- Contrôles visuels des captures Windows et des rapports avec images.
- Signature de code : aucun certificat de signature disponible ; paquet candidat non signé.
- Les rôles sont actuellement mono-utilisateur (compte Windows) ; pas de séparation administrateur/auditeur/responsable.
- Contacts/établissements détaillés, recherche globale, seuils de risques paramétrables, dépendances d’actions et export complet d’une mission restent incomplets.
- IA : adaptateur compatible Chat Completions testé avec faux fournisseur ; appel réel non effectué. Les réponses sont du texte modifiable, sans schéma de sortie strict. Pas de pseudonymisation automatique.
- La bibliothèque utilise un éditeur JSON validé, pas un concepteur graphique de questions. Pas de règles conditionnelles.
- Le rapport contient les versions PNG des schémas enregistrés ; il ne sélectionne pas encore une version précise par schéma.
- PDFsharp 6.2.0 MIT épinglé ; la migration vers les nouvelles versions nécessite une décision de licence.

Prochaine action : lire la CI de cette branche et corriger les erreurs Windows / installateur, puis poursuivre les écarts avant publication finale. Ne pas annoncer la V1 complète.

## Correction Windows du 23 octobre 2026

Le run 39799037413 a compilé puis exécuté 34 tests : 32 réussis, 2 en échec. L’exception concernait le fichier temporaire `cdg-backup-*/auditpilot.db` au calcul SHA-256, et non une copie directe de la base active. `Dispose` laissait le handle SQLite temporaire dans le pool.

Correction : `Pooling=false` sur source et destination de BackupDatabase, source ouverte en lecture seule, fermeture effective avant le hachage et la compression. Suppression du vidage global des pools dans Create, désormais inutile. Test supplémentaire : deux sauvegardes successives avec une connexion source ouverte, puis restauration et lecture. Résultat du nouveau pipeline à vérifier avant validation.

Validation du correctif `596b968` : [run Windows 39905200125](https://github.com/dforrez-del/CDG-AuditPilot/actions/runs/39905200125), terminé avec succès le 23 octobre 2026. 35 tests réussis, 0 échec, dont les deux tests initialement défaillants et le test de sauvegardes successives avec connexion source ouverte. Compilation, publication x64, parcours WPF automatisé, création de l’installateur Inno Setup, installation silencieuse et parcours après installation réussis. Deux avertissements non bloquants (NU1510 et xUnit2029) subsistent. Ce succès valide le correctif et la chaîne de packaging, pas tous les écarts fonctionnels de la V1 listés ci-dessus.

Installateur candidat : https://github.com/dforrez-del/CDG-AuditPilot/actions/runs/39905200125/artifacts/12480790679
Résultats et captures : https://github.com/dforrez-del/CDG-AuditPilot/actions/runs/39905200125/artifacts/12481035468


## Reprise ergonomique du 23 octobre 2026 — candidat rc2

Référence visuelle : capture MicroPilot fournie par le propriétaire. Conservation du socle WPF et des données ; menu latéral bleu nuit, accents turquoise, tableau de bord calculé sur les dossiers, dossier actif permanent, commandes contextuelles, colonnes métier explicites et suppression de la barre globale encombrée. Les six écrans historiques restent accessibles. Aucun contenu fictif dans le tableau de bord normal.

Compléments : historique des instantanés avec utilisateur Windows, recherche globale, export chiffré d’une mission, analyse IA structurée avec références vérifiées et masquage optionnel courriels/téléphones. Ce masquage ne constitue pas une anonymisation complète. Contacts/sites, seuils et dépendances sont implémentés ; ils nécessitent encore une recette métier approfondie. L’éditeur de bibliothèque reste JSON.

Validation locale de cette révision : compilation Release sans avertissement ni erreur ; 39/39 tests réussis, y compris le refus de champs IA explicitement nuls. Windows, installation et captures : résultats à consigner après la CI. Le précédent résultat Windows 35/35 ne valide pas cette nouvelle interface.

Prochaine action : contrôler le pipeline rc2 et ses captures réelles, corriger toute anomalie et livrer le candidat. Les rôles séparés, règles conditionnelles, signature de code et recette métier complète restent ouverts. Ne pas qualifier ce candidat de V1 complète.


## Vérification confirmée le 24 octobre 2026 — rc2

Code testé : `1f2901b70911e0be786901beb79ade35ddeb9437`.
Pipeline du 23 octobre : https://github.com/dforrez-del/CDG-AuditPilot/actions/runs/39907521949

- Build Windows : réussi, zéro erreur.
- Tests Windows : 39 réussis, zéro échec, zéro ignoré.
- Publication autonome Windows x64 : réussie.
- Parcours WPF automatisé, exports et persistance : réussi.
- Installateur Inno Setup puis installation silencieuse et lancement installé : réussis.
- Captures WPF examinées ; menu Paramètres correctement sélectionné, largeur minimale des constats préservée. Les petites fenêtres nécessitent du défilement ; la lisibilité de tous les contenus longs reste à compléter en recette métier.

Installateur : https://github.com/dforrez-del/CDG-AuditPilot/actions/runs/39907521949/artifacts/12481572036
Résultats TRX, captures et exports : https://github.com/dforrez-del/CDG-AuditPilot/actions/runs/39907521949/artifacts/12481781092

Le paquet est un candidat rc2 non signé, pas une V1 intégralement réceptionnée. Restent notamment : concepteur graphique de questionnaires et règles conditionnelles, séparation des rôles, recette complète des quatre modèles d’audit et appel IA réel. Les copies des rapports ont été produites automatiquement ; leur mise en page complète n’a pas été réceptionnée visuellement dans cette passe.

Prochaine opération de développement : remplacer l’édition JSON seule de la bibliothèque par un éditeur de questions métier, tout en conservant le format importé et le versionnement existants, puis tester les reprises d’entretiens. Conserver le pipeline Windows comme contrôle de livraison.


## Évolution rc3 — 24 octobre 2026

Éditeur visuel de questionnaires : sections et questions modifiables, ordre, neuf types de réponses, options multilignes, validation réutilisant QuestionnaireFormat, identifiants conservés, fichiers de versions créés sans écrasement, avertissement avant abandon. Sélection directe depuis un entretien, sans export/réimport obligatoire. Le format JSON v1 et les instantanés d’entretiens restent compatibles.

Tests ajoutés : modification sans altération de la source, stabilité des identifiants/options, refus de sections vides et options dupliquées, ordre de sections homonymes et changement de type. Le smoke WPF crée deux versions, vérifie la première et produit une capture de l’éditeur. Résultats de compilation/CI à compléter après exécution. Les règles conditionnelles et les rôles séparés restent ouverts.
