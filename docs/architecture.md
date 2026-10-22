# Architecture et étapes

Le prompt maître est prioritaire sur l’ancien cahier des charges : conseil organisationnel, flux, stocks physiques et outils Excel ; pas de tenue, révision ou certification des comptes.

Domain est indépendant. Application dépend de Domain. Infrastructure, AI, Reporting et Templates dépendent d’Application. Desktop utilise Application et référence Infrastructure uniquement pour l’assemblage des services. Tests référence Infrastructure et ses dépendances métier.

Un contexte EF Core par opération, jamais de contexte conservé dans un ViewModel. Les créations et événements de journal sont enregistrés ensemble. Aucune suppression en cascade. Référence mission normalisée en majuscules, index unique et contrôle de clé étrangère.

La migration initiale est écrite explicitement avec un snapshot EF Core. Les migrations suivantes doivent être générées et revues ; ne jamais remplacer une base existante par EnsureCreated.

La première étape est volontairement un parcours réduit vérifiable. Les étapes suivantes enrichissent les fiches clients/missions, puis ajoutent questionnaires versionnés, entretiens et autosauvegarde, preuves/constats, processus/stocks/Excel, actions, exports et enfin IA avec consentement.

Les versions NuGet indiquées sont des versions initiales explicites, pas une affirmation qu’il s’agit des derniers correctifs disponibles. Le contrôle des licences et des vulnérabilités doit précéder la diffusion commerciale. Aucun moteur PDF n’est sélectionné dans cette étape.
