# Recette import / API / flux

## Vérification automatisée locale — 22 octobre 2026

Compilation croisée WPF et projets .NET 10 : zéro erreur et zéro avertissement. Tests d’import, de réponses typées, de conservation des sessions, d’autorisation API, du contenu effectivement envoyé, de rejet des réponses tronquées et de disposition de flux. 19 tests automatisés réussis sur 19, y compris la migration depuis la première version et la conservation du client. Aucun appel externe réel dans les tests.

## À vérifier sur Windows

1. Ouvrir une mission, importer examples/questionnaire-audit.json, saisir puis enregistrer les réponses.
2. Fermer puis rouvrir, retrouver les réponses. Importer un deuxième questionnaire et vérifier que le premier reste présent.
3. Essayer un fichier avec identifiant de question dupliqué, type inconnu ou condition non prise en charge : aucun import partiel.
4. Configurer un service API autorisé. Vérifier destination et contenu de l’aperçu ; Annuler ne transmet rien.
5. Avec données fictives et une clé valide, autoriser un appel : vérifier la proposition, la persistance et les références de questions. Ce scénario nécessite un compte fournisseur et peut être facturé.
6. Vérifier qu’un délai dépassé ou quota épuisé n’entraîne pas de nouvel appel automatique.
7. Importer examples/schema-flux.json, modifier une étape, générer, enregistrer et rouvrir. Vérifier les branches et les flèches de retour.
8. Importer une image PNG puis JPEG, enregistrer et retrouver l’image dans la même mission ; vérifier qu’une autre mission ne la liste pas.
9. Exporter un schéma en PNG et l’ouvrir pour vérifier le rendu. Les schémas complexes peuvent nécessiter une simplification pour réduire les croisements.
10. Tester la migration depuis la version précédente, sans perte du client et de la mission.

Non vérifié dans l’environnement Linux : ouverture des fenêtres WPF, rendu PNG, interaction clavier et DPI, ni réponse réelle d’un fournisseur IA. Les tests graphiques et API réels ne sont pas déclarés réussis.
