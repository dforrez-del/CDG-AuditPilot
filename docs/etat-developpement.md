# État vérifié — reprise du 22 octobre 2026

Base examinée : `main` au commit d8ea284 et branche `codex/socle-clients-missions` au commit d749b52. Ces branches avaient divergé : la première utilisait JSON et six onglets ; la seconde apportait EF Core/SQLite, les entretiens détaillés et les flux. L’interface de main et ses fonctions ont été conservées et les modules de la branche précédente réintégrés, sans repartir de zéro.

## Vérifications locales

- SDK .NET 10.0.402 : compilation de la solution en Release, 0 erreur, 0 avertissement.
- 34 tests xUnit réussis : persistance, migration, imports, API simulée, flux, preuves, liens inter-missions, concurrence, chiffrement/restauration, DOCX/XLSX/PDF.
- NuGet signalait Pkcs transitif : épinglé à 10.0.13, version corrigée selon GHSA-846f-f764-76mx et GHSA-2fmv-rv2c-59x7.
- Ces résultats Linux ne prouvent pas le fonctionnement visuel Windows.

## Ajouts intégrés

Migration du JSON historique sans suppression de l’original ; conservation des dossiers SQLite existants ; sauvegardes transactionnelles et contrôle de révision ; bibliothèque de questionnaires JSON à versions immuables ; entretiens détaillés et autosauvegarde ; preuves copiées et vérifiées SHA-256 ; rattachement aux constats et revue humaine ; édition des actions et missions ; clés API DPAPI ; sauvegardes AES-GCM manuelles et automatiques ; restauration en dossier intermédiaire avec manifeste ; exports DOCX, PDF, XLSX ; diagrammes PNG/SVG avec inclusion des PNG conservés dans les rapports.

## Livraison non acquise

La CI Windows doit encore être exécutée et son résultat lu. Elle compile, teste, lance un parcours WPF automatisé, produit des captures et rapports, construit un installateur Inno Setup x64 puis teste son installation silencieuse. Aucun installateur n’est déclaré validé avant ces contrôles.

## Écarts restant à lever

- Contrôles visuels des captures Windows et des rapports avec images.
- Signature de code : aucun certificat de signature disponible ; paquet candidat non signé.
- Les rôles sont actuellement mono-utilisateur (compte Windows) ; pas de séparation administrateur/auditeur/responsable.
- Contacts/établissements détaillés, recherche globale, seuils de risques paramétrables, dépendances d’actions et export complet d’une mission restent incomplets.
- IA : adaptateur compatible Chat Completions testé avec faux fournisseur ; appel réel non effectué. Les réponses sont du texte modifiable, sans schéma de sortie strict. Pas de pseudonymisation automatique.
- La bibliothèque utilise un éditeur JSON validé, pas un concepteur graphique de questions. Pas de règles conditionnelles.
- Le rapport contient les versions PNG des schémas enregistrés ; il ne sélectionne pas encore une version précise par schéma.
- PDFsharp 6.2.0 MIT épinglé ; la migration vers les nouvelles versions nécessite une décision de licence.

Prochaine action : lire la CI de cette branche et corriger les erreurs Windows / installateur, puis poursuivre les écarts avant publication finale. Ne pas annoncer la V1 complète.
