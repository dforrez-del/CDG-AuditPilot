# CDG AuditPilot — guide du candidat V1

## Installation et données

Le paquet candidat s’installe pour le compte Windows courant, sans droits administrateur. Il inclut le runtime .NET x64. Les données sont dans `%LOCALAPPDATA%\CapDigitalGestion\AuditPilot`. La désinstallation conserve ce dossier. Fermer l’application avant une mise à jour et réaliser une sauvegarde manuelle.

Les fonctions métier sont locales. L’IA seule nécessite une connexion. La base SQLite n’est pas chiffrée ; utiliser les protections du compte Windows et du disque. Ne pas partager le dossier sur un lecteur réseau. Un seul processus peut être ouvert pour le compte courant.

## Parcours

1. Créer un client dans Clients et missions, le sélectionner, créer sa mission puis la sélectionner.
2. Utiliser Mission pour préciser référence, périmètre et statut.
3. La Bibliothèque permet de créer et modifier visuellement les sections et questions, d’importer/exporter du JSON et d’enregistrer une nouvelle version. Les anciens fichiers ne sont pas écrasés. Les modèles initiaux sont des points de départ courts à enrichir.
4. Entretiens détaillés : choisir un questionnaire dans la bibliothèque ou importer un fichier JSON, puis renseigner interlocuteur, observations et réponses. Cocher Non applicable si nécessaire. Autosauvegarde toutes les trois secondes lorsque les réponses sont valides. Le message indique les erreurs ; un brouillon invalide n’est pas enregistré automatiquement.
5. L’onglet Entretiens de la fenêtre principale permet également de conserver des notes libres.
6. Preuves : indiquer une origine puis importer les fichiers. Une copie locale est conservée ; le fichier source reste intact. Vérifier l’empreinte ou exporter une copie. Limite 100 Mo par fichier.
7. Ajouter un constat, le sélectionner puis Réviser le constat. Renseigner critère, cause, conséquence et preuves. La validation humaine enregistre le nom du compte Windows.
8. Ajouter une action, la sélectionner puis Modifier l’action pour compléter échéance, priorité, charge, coût et contrôle d’efficacité.
9. Schémas modifiables : créer/importer des étapes et liaisons, générer, enregistrer. Les images PNG et versions SVG sont conservées avec la mission. Les images PNG sont incluses dans les rapports.
10. Word / PDF / Excel : choisir le format. Les constats non validés portent la mention BROUILLON. L’Excel conserve les valeurs commençant par = comme texte, sans formules exécutables.

## IA

Dans Paramètres IA / rapport, enregistrer la clé : elle est chiffrée par DPAPI pour votre compte Windows. Dans Entretiens détaillés, renseigner l’URL complète HTTPS compatible `/chat/completions` et l’identifiant de modèle fourni par le prestataire. Lire le contenu exact avant chaque autorisation. Les pièces jointes brutes ne sont pas envoyées. Les textes ne sont pas anonymisés automatiquement.

Les résultats sont des propositions modifiables, non validées. Ils ne sont pas intégrés automatiquement dans un rapport final. Vérifier les affirmations et les sources, puis formaliser les constats et actions retenus. Les coûts éventuels sont ceux du fournisseur API.

## Sauvegardes

Sauvegarder produit une archive AES-GCM protégée par un mot de passe d’au moins 12 caractères. Conserver ce mot de passe hors du poste : il n’est pas récupérable. Les clés API sont exclues. Une sauvegarde automatique est créée toutes les cinq minutes ; les dix dernières sont conservées dans le dossier voisin `AuditPilot-AutoBackups`. Leur clé est protégée pour le compte Windows courant : elles ne remplacent pas une sauvegarde manuelle exportable.

Restaurer vérifie le chiffrement, les empreintes et l’intégrité SQLite avant remplacement. Le dossier courant est conservé sous un nom `.before-restore-*`. L’application se ferme ; la relancer. Restaurer auto utilise la clé locale et exige le même compte Windows.

## Limites

Consulter `etat-developpement.md` avant toute utilisation de production. Le candidat n’est pas signé. La séparation des rôles, certains éléments du cahier des charges et les contrôles de recette ne sont pas encore tous validés.


## Interface rc2

Le tableau de bord présente les missions actives, clients, constats à valider et actions ouvertes. Ses compteurs couvrent tous les dossiers. Double-cliquez une mission pour l’activer. Le bandeau « Dossier actif » rappelle toujours le contexte de travail.

Utilisez le menu à gauche pour avancer dans la mission. Les boutons au-dessus du contenu proposent uniquement les opérations de l’écran courant. Dans Questionnaires, ouvrez la bibliothèque puis l’entretien guidé. Dans Constats, sélectionnez une ligne puis « Réviser le constat sélectionné » pour associer les preuves et valider la cotation. Dans Plan d’actions, sélectionnez une action pour compléter son échéance et ses dépendances.

Les paramètres, sauvegardes et restaurations se trouvent en bas du menu. Une restauration remplace le dossier ouvert et conserve son ancienne copie ; l’export d’une mission est une sauvegarde de ce seul dossier, pas une fusion de dossiers.


## Éditeur visuel rc3

Dans Questionnaires → Bibliothèque, créez un questionnaire ou ouvrez une version. Modifiez le titre et les sections dans les cellules ; sélectionnez une section pour afficher ses questions. Les flèches changent l’ordre. Chaque question possède un libellé, un type de réponse et une case Obligatoire. Pour un choix unique ou multiple, saisissez une option par ligne dans la cellule Choix. Les autres types ne publient pas d’options.

« Enregistrer une version » crée un nouveau fichier, sans remplacer les versions existantes ni modifier les entretiens déjà commencés. Les identifiants des questions conservées restent stables. Les sections vides et les choix incohérents empêchent l’enregistrement. Les réponses obligatoires restent compatibles avec les brouillons incomplets ; cette case ne bloque pas la sauvegarde de l’entretien.

Dans un entretien, « Choisir dans la bibliothèque » permet d’ouvrir ou de préparer un questionnaire puis « Enregistrer et utiliser » en conserve un instantané dans la mission. L’import et l’export JSON restent disponibles. Fermer ou changer de version demande confirmation lorsque le brouillon a été modifié.


## Saisie des entretiens rc4

Les questions Oui/Non, choix unique et notes proposent une liste. Les choix multiples utilisent des cases à cocher. Pour une date, saisissez AAAA-MM-JJ ou ouvrez le calendrier. Les nombres utilisent le format français (12,5). Les textes longs acceptent plusieurs lignes.

Une valeur ancienne incompatible est conservée et signalée en rouge : corrigez-la ou utilisez « Effacer la réponse ». Une liste vide ne signifie pas que la valeur signalée a été supprimée. Le contrôle Non applicable désactive la saisie ; comme précédemment, une réponse non applicable est enregistrée vide. Les observations restent séparées. La sauvegarde automatique refuse les réponses incompatibles et conserve le dernier enregistrement valide.
