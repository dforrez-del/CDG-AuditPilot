# Questionnaires ChatGPT, analyse par API et schémas de flux

## Parcours utilisateur

1. Sélectionner une mission dans la liste des missions du client.
2. Ouvrir « Ouvrir le questionnaire de la mission ».
3. Importer le fichier JSON préparé par ChatGPT selon le format ci-dessous. Chaque import crée une nouvelle session ; il ne remplace pas les réponses existantes.
4. Compléter les réponses puis cliquer sur Enregistrer. Les types de réponses sont contrôlés ; les brouillons incomplets sont autorisés. Les versions enregistrées sont conservées dans SQLite. L’interface permet de reprendre la dernière version, pas encore de parcourir tout l’historique.
5. Dans Analyse par API, saisir URL, modèle et clé du fournisseur.
6. Vérifier le contenu exact et la destination dans la fenêtre d’autorisation. Annuler n’effectue aucun appel.
7. La réponse est une proposition non validée. Les données envoyées, le modèle, l’URL et la date d’autorisation sont conservés avec cette proposition. Les références suggérées par le modèle doivent être vérifiées humainement.

## Prompt à copier dans ChatGPT

Prépare un questionnaire d’audit organisationnel pour [activité et périmètre à préciser]. Retourne uniquement un fichier JSON UTF-8 valide, sans commentaires ni bloc Markdown, conforme au modèle `examples/questionnaire-audit.json` de CDG AuditPilot. Structure exacte : schemaVersion (1), title, sections. Chaque section contient title et questions. Chaque question contient obligatoirement id, label, type, required et options. Les identifiants des questions doivent être uniques dans le fichier. Types admis : text, longText, yesNo, singleChoice, multipleChoice, scale0to5, scale0to10, number, date. options est un tableau de choix uniquement pour singleChoice et multipleChoice, sinon []. Ne pas inclure de point-virgule dans les choix. Limites : 50 sections, 500 questions, titre 200 caractères, identifiant 80, libellé 4000. Ne pas ajouter de champs ou de conditions. Employer « services commerciaux ». Étudier l’organisation, les responsabilités, les documents, les outils, Excel et les mouvements physiques de stocks. Exclure la tenue, la révision, la certification et la valorisation comptable. Ne pas inventer de faits sur l’entreprise. Les questions doivent permettre de distinguer les déclarations et les justificatifs disponibles.

Le fichier doit être réellement téléchargé en .json. Le moteur ne lit pas encore les fichiers Word/PDF ni les questionnaires rédigés librement. Les champs inconnus, questions conditionnelles et types non pris en charge sont rejetés avec un message, jamais ignorés silencieusement.

Saisie actuelle : zones de texte avec contrôle du type. Oui/Non : écrire exactement Oui ou Non. Choix multiples : séparer les choix par un point-virgule. Date : AAAA-MM-JJ. Nombre : format français. Les cases à cocher et autres contrôles spécialisés viendront ultérieurement.

## Connexion IA

Premier adaptateur : API compatible Chat Completions, via HTTPS, messages system/user et max_completion_tokens=4000. URL OpenAI proposée : https://api.openai.com/v1/chat/completions. Aucun modèle n’est présélectionné : renseigner un modèle disponible pour votre compte et compatible avec ce contrat. La clé reste dans la fenêtre pendant la session et n’est jamais écrite dans la base ou les journaux. Les paramètres URL et modèle ne sont pas encore persistés.

L’appel est limité à 120 secondes et la requête à 250 Ko. Aucun renvoi automatique après échec, aucune redirection HTTP suivie. La prévisualisation est exactement le corps de la requête envoyée. Les fichiers joints, coordonnées du client et identifiants internes de mission ne sont pas ajoutés automatiquement. Les textes saisis peuvent toutefois contenir des informations personnelles : aucune anonymisation automatique n’est implémentée.

Anthropic et Gemini natifs ne sont pas encore implémentés. L’interface IAuditAnalysisProvider permet leur ajout. Une compatibilité avec un service tiers doit être testée : « compatible OpenAI » ne garantit pas tous les modèles et paramètres.

Source technique : OpenAI, Create chat completion, documentation consultée le 22 octobre 2026 : https://developers.openai.com/api/reference/resources/chat/subresources/completions/methods/create

Aucun appel réel n’a été réalisé pendant le développement ; tests avec transport HTTP simulé uniquement.

## Schémas de flux

Depuis une mission, ouvrir « Schémas de flux ».

- Importer une image PNG/JPEG : image statique consultable et conservée dans le dossier de mission, non transformée en objets éditables.
- Importer un JSON `examples/schema-flux.json` : étapes, services et liaisons deviennent éditables dans les grilles.
- Créer un nouveau schéma : saisir les étapes, leurs identifiants et services, puis les liaisons avec documents ou conditions.
- Cliquer sur Générer le schéma puis ouvrir l’onglet Schéma. La disposition est automatique ; branches et boucles sont prises en charge, sans garantie d’absence de croisement sur les graphes complexes.
- Enregistrer conserve une nouvelle version dans le dossier local de la mission. Exporter PNG crée une image utilisable dans un document.

Format flux : schemaVersion=1, title, nodes=[{id,label,service}], edges=[{from,to,label}]. Les identifiants des étapes sont uniques ; les extrémités des liaisons doivent exister. Limites : 30 étapes, 60 liaisons. Libellé étape 120 caractères, service et libellé liaison 80. Un fichier JSON peut aussi être préparé par ChatGPT à partir d’un processus décrit, puis relu avant import.

La génération graphique est déterministe et locale. La génération du contenu d’un processus par l’API IA depuis un entretien n’est pas encore intégrée. Import SVG/PDF/Mermaid, export vectoriel/PDF et insertion automatique dans les rapports restent à développer.

Stockage : SQLite pour les questionnaires, réponses et propositions IA ; `%LOCALAPPDATA%/CapDigitalGestion/AuditPilot/Missions/<mission>/Flows` pour les schémas. Une future sauvegarde doit inclure la base ET ce dossier. Ne pas copier uniquement la base en croyant sauvegarder les schémas.
