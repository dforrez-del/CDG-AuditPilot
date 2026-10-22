# CDG AuditPilot — Plan d'exécution V1

Statut : plan de développement, non preuve de réalisation. Référence : cahier des charges V1 et demandes complémentaires du 22/10/2026.

## Règles de livraison

- Conserver le socle clients/missions/SQLite existant ; éviter les régressions.
- Chaque module comprend modèle métier, persistance/migration, services, ViewModel WPF, validation, tests unitaires et intégration.
- Ne pas déclarer une fonctionnalité terminée sans compilation et tests vérifiés.
- Utiliser uniquement des données fictives jusqu'à validation RGPD, chiffrement, sauvegarde/restauration et contrôles d'accès.
- Ne jamais transmettre de documents ou réponses à l'IA sans prévisualisation et consentement explicite.

## Lot 2 — Questionnaires et entretiens

1. Entités QuestionnaireTemplate, QuestionnaireVersion, Section, Question, Choice, Interview, Answer, InterviewEvidenceLink. Identifiants UUID, version immuable publiée, statut et horodatages UTC.
2. Import JSON versionné depuis un questionnaire préparé par ChatGPT : validation JSON Schema, taille maximale, types de questions (texte, choix unique, choix multiple, nombre, note, oui/non), règles de branchement simples, prévisualisation des erreurs, aucune exécution de contenu importé.
3. Création et édition de questionnaires WPF, duplication/versionnement, liaison à une mission, sauvegarde automatique des réponses et reprise après redémarrage.
4. Entretiens multi-interlocuteurs avec service/fonction, observations, non-applicable et pièces référencées.
5. Tests : import valide/invalide, version figée, persistance, reprise, questionnaire multi-sections, aucune perte de réponse.

## Lot 3 — Preuves, constats, risques et actions

1. Preuves importées dans un espace local contrôlé, nom sécurisé, SHA-256, provenance et liaison aux constats.
2. Constats avec critère, situation, cause présumée, conséquences, références des preuves, état de revue et validation humaine.
3. Risque = impact (1–5) × probabilité (1–5) ; seuils configurables, justification humaine.
4. Plan d'actions avec responsable, priorité, échéance, charge, coût, statut, contrôle d'efficacité.
5. Journal de modification et tests de traçabilité.

## Lot 4 — IA cloud configurable et schémas

1. Interface IAIProvider indépendante du fournisseur ; adaptateur API, tests avec fournisseur simulé, paramètres et secrets protégés.
2. Consentement avant envoi, minimisation/pseudonymisation, exclusion par défaut des pièces brutes, journal sans secrets.
3. Sorties IA structurées et validées par schéma : synthèse, divergences, questions complémentaires, constats proposés et chapitres de rapport. Aucune publication automatique.
4. Cartographie des flux : entités ProcessMap, Node, Edge, version ; éditeur visuel WPF ; import/export d'un format structuré documenté ; génération IA d'une proposition de schéma à partir d'entretiens, avec validation manuelle.
5. Prévoir les flux achats/fournisseurs, ventes, trésorerie, stocks, inventaires, documents, messageries et SI.

## Lot 5 — Restitution, sécurité, distribution

1. Rapport Word modifiable, plan d'actions Excel, export PDF local vérifié ; modèles personnalisables.
2. Sauvegarde chiffrée, restauration testée incluant base et pièces ; rétention/purge.
3. Vérification des rôles, contrôle des chemins, imports malveillants, journaux, mode hors ligne et gestion d'erreurs.
4. Pipeline Windows : restore, build, tests, publication x64, artefact ; installateur et signature à traiter selon certificats disponibles.
5. Recette bout-en-bout sur une mission fictive couvrant les quatre modèles d'audit et les flux de stocks.

## Critère final

Ne demander une recette utilisateur qu'après livraison d'un paquet Windows exécutable et passage des tests automatisés ; signaler séparément tout contrôle manuel ou fonction non validé.
