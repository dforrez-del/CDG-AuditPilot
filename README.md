# CDG AuditPilot Windows

Application locale de conseil organisationnel et de digitalisation — Cap Digital Gestion.

## État : étape 1, version de développement

Implémenté : création et consultation des clients, création et consultation des missions par client, validation des saisies, référence de mission unique, persistance SQLite avec migration initiale, journal des créations transactionnel, protection contre la fermeture avec saisie non enregistrée et logo CDG.

Pas encore implémenté : modification/archivage des fiches, fiches métier complètes, questionnaires, entretiens, autosauvegarde, cartographie, stocks, inventaire Excel, risques, actions, rapports, IA, sauvegarde/restauration, rôles applicatifs, thèmes, installateur et mises à jour signées. Les projets AI, Templates et Reporting sont des réservations d’architecture, pas des modules fonctionnels.

## Développement

Windows x64, SDK .NET 10 et Visual Studio 2026 (18.0 minimum pour cibler .NET 10).

```powershell
dotnet restore CDG.AuditPilot.sln
dotnet build CDG.AuditPilot.sln -c Release
dotnet test tests/CDG.AuditPilot.Tests -c Release
dotnet run --project src/CDG.AuditPilot.Desktop
```

## Exécutable de prévisualisation

Après réussite du workflow GitHub Actions, télécharger l’artefact `AuditPilot-Windows-x64-preview`, extraire entièrement le ZIP puis lancer `CDG.AuditPilot.Desktop.exe`. Ce paquet autonome n’est pas un installateur et n’est pas signé.

## Données locales

`%LOCALAPPDATA%\CapDigitalGestion\AuditPilot\auditpilot.db`

Aucune donnée client ni clé API n’est incluse dans ce dépôt. Aucun appel IA n’est implémenté. Base non chiffrée : cette première version doit être évaluée avec des données fictives. Les droits du compte Windows protègent le dossier, sans contrôle de rôles applicatifs à ce stade.

Enregistrer explicitement chaque formulaire. La fermeture avertit si une saisie reste non enregistrée. Les messages techniques locaux ne contiennent ni saisies métier ni secrets. Ne pas utiliser la base sur un partage réseau. La restauration sera livrée et testée avant une utilisation en production.

## Validation

Voir `docs/recette-etape-1.md` et les résultats GitHub Actions. Une compilation réussie ne remplace pas la recette visuelle Windows.
