# Architecture et étapes

Le prompt maître est prioritaire sur l’ancien cahier des charges : conseil organisationnel, flux, stocks physiques et outils Excel ; pas de tenue, révision ou certification des comptes.

Domain est indépendant. Application dépend de Domain. Infrastructure, AI, Reporting et Templates dépendent d’Application. Desktop utilise Application et référence Infrastructure uniquement pour l’assemblage des services. Tests référence Infrastructure et ses dépendances métier.

Un contexte EF Core par opération, jamais de contexte conservé dans un ViewModel. Les créations et événements de journal sont enregistrés ensemble. Aucune suppression en cascade. Référence mission normalisée en majuscules, index unique et contrôle de clé étrangère.

La migration initiale est écrite explicitement avec un snapshot EF Core. Les migrations suivantes doivent être générées et revues ; ne jamais remplacer une base existante par EnsureCreated.

La première étape est volontairement un parcours réduit vérifiable. Les étapes suivantes enrichissent les fiches clients/missions, puis ajoutent questionnaires versionnés, entretiens et autosauvegarde, preuves/constats, processus/stocks/Excel, actions, exports et enfin IA avec consentement.

Les versions NuGet indiquées sont des versions initiales explicites, pas une affirmation qu’il s’agit des derniers correctifs disponibles. Le contrôle des licences et des vulnérabilités doit précéder la diffusion commerciale. Aucun moteur PDF n’est sélectionné dans cette étape.

## Extension questionnaires, IA et schémas

InterviewDocuments conserve des snapshots append-only du questionnaire et des réponses par session, liés par clé étrangère à la mission. Le contenu JSON est validé avant enregistrement ; l’ancien client et les missions restent intacts lors de la migration. Cette persistance intermédiaire devra évoluer vers les tables normalisées du cahier des charges lors de l’éditeur avancé.

L’identifiant de migration initial historique (202610220001_InitialSchema) est conservé pour les bases déjà installées ; il n’a pas le format EF standard à 14 chiffres et ne doit pas être renommé. La migration suivante utilise le format standard. La recette de mise à niveau reconstruit le schéma et l’historique exacts de la première version, puis applique la nouvelle migration.

Les schémas structurés et les images sont conservés en fichiers locaux par UUID de mission, avec une nouvelle version à chaque enregistrement. Ils devront être inclus dans les sauvegardes et exports complets futurs.
