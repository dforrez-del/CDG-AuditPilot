# Reprise technique

Les modules Domain/Application/Infrastructure/AI/Reporting proviennent de la branche d749b52 ; la fenêtre principale et les opérations libres proviennent de main d8ea284. Le modèle historique AuditEvidence/Hasher est conservé. EvidenceStore ajoute copie gérée et contrôle des chemins.

SQLite utilise les migrations InitialSchema, InterviewDocuments et WorkspaceStates. Clients et missions restent des tables normalisées ; questionnaires/analyses sont des instantanés immuables liés aux missions. Les autres données de la fenêtre principale sont conservées dans WorkspaceStates sous forme de document JSON transactionnel avec numéro de révision et journal AuditEvents. Ce compromis préserve le code initial ; une normalisation plus fine reste possible sans changer les interfaces utilisateur.

CaseworkStore charge les clients/missions depuis les tables, contrôle les associations avant sauvegarde et refuse les mises à jour d’un instantané obsolète. La migration de data.json est effectuée une seule fois, avec copie .before-sqlite. Le fichier JSON original demeure inchangé.

BackupService copie SQLite via BackupDatabase, archive les fichiers et un manifeste SHA-256, puis chiffre en AES-256-GCM avec PBKDF2-SHA256 (210000 itérations, sel 16 octets, nonce 12 octets). La restauration déchiffre et valide dans un dossier intermédiaire avant échange de répertoires. Les données temporaires de sauvegarde sont locales et effacées en finally.

Build : `dotnet build CDG.AuditPilot.sln -c Release -m:1`.
Tests : `dotnet test tests/CDG.AuditPilot.Tests -c Release -m:1`.
Publication : `dotnet publish src/CDG.AuditPilot.Desktop -c Release -r win-x64 --self-contained true`.
Le workflow Windows inclut un smoke test WPF isolé via --smoke-test et AUDITPILOT_TEST_ROOT, puis ISCC et installation silencieuse.

Dépendances de rapport : OpenXML 3.3.0, PDFsharp 6.2.0 (MIT dans le package), Pkcs 10.0.13. Les versions suivantes de PDFsharp changent de licence. La police PDF est Arial sur Windows, DejaVu Sans pour les tests Linux. Aucun service de conversion cloud ni installation Office requise.
