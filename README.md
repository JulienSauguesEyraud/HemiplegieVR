# Application VR de Thérapie Miroir — Rééducation de l'Hémiplégie

> **Cadre du projet :** Projet de 4ème semestre — BUT Informatique Graphique.
> **Partenariat de conception :** Établi avec des ergothérapeutes de l'Hôpital Émile-Roux (Le Puy-en-Velay) pour la définition des besoins fonctionnels.
> **Matériel :** Meta Quest 3 (Hand Tracking optique complet, sans manettes).

Ce projet est une application de réalité virtuelle conçue comme un prototype d'outil d'accompagnement pour les ergothérapeutes dans la rééducation de patients atteints d'hémiplégie. 

En s'appuyant sur le principe neuro-scientifique de la **thérapie miroir**, l'application capte les mouvements du bras sain du patient via les capteurs optiques du casque et les reproduit instantanément sur son bras affecté de manière virtuelle, stimulant ainsi la plasticité cérébrale sans nécessiter la manipulation de contrôleurs physiques.

---

## Fonctionnalités & Approche Asymétrique

L'application propose une interface double pour s'adapter parfaitement au contexte de soin :

* **Côté Patient (Dans le casque) :** Immersion totale dans un salon virtuel semi-réaliste aux tons pastels et apaisants. L'environnement est totalement épuré d'interfaces visuelles (**No-UI**) pour maximiser la concentration et l'engagement thérapeutique.
* **Côté Soignant (Sur l'écran PC) :** Un tableau de bord complet permettant au praticien de configurer et de modifier la séance en temps réel et de manière dynamique.

### Activités incluses
1. **Mouvement libre :** Pour travailler l'amplitude, la symétrie et la réappropriation du membre dans l'espace.
2. **Manipulation d'objets :** Exercices fonctionnels de préhension avec divers objets du quotidien (livre, vase, brosse, tasse, ...).

### Panneau de Contrôle du Praticien (Interface PC)
* **Gestion de l'exercice :** Changement d'activité instantané en un clic.
* **Inversion de la latéralité :** Sélection dynamique de la main à tracker (gauche ou droite) selon le côté hémiplégique du patient.
* **Inclusivité & Accessibilité :** Choix parmi 4 teintes de peau pour renforcer l'identification du patient à ses avatars de mains.
* **Affichage adaptatif :** Option pour afficher ou masquer les avant-bras (pour se concentrer exclusivement sur les mains).
* **Gestion de l'espace :** Recentrage instantané de la caméra (*"Réinitialiser le miroir"*) et bouton pour replacer les objets à leur position de départ.

---

## Configuration & Lancement

L'application est fournie sous forme d'exécutable standalone (`.exe`). Elle est optimisée pour tourner sur n'importe quel PC moderne standard capable de supporter la VR de base.

1. Connectez le **Meta Quest 3** au PC (via **Meta Horizon Link** par câble ou **Air Link**).
2. Activez le mode Link dans le casque.
3. Lancez l'exécutable sur le PC.
4. **Utilisation :** L'interface de contrôle du soignant s'affiche sur l'écran du PC, tandis que le flux VR épuré est envoyé directement dans le casque du patient.

*Note : Assurez-vous que le suivi des mains (Hand Tracking) est bien activé dans les paramètres globaux de votre casque Meta Quest.*

---

## Distribution & Statut du projet
Ce projet est un prototype fonctionnel développé dans un cadre universitaire. En raison des protocoles stricts d'analyses scientifiques et réglementaires indispensables au milieu clinique, l'application n'est pas déployée en conditions réelles à l'hôpital. 
