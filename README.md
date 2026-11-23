<div align="center">

# 🧠 HémiplégieVR

*Application VR de Thérapie Miroir pour la Rééducation de l'Hémiplégie.*  
*VR Mirror Therapy Application for Hemiplegia Rehabilitation.*

![Unity](https://img.shields.io/badge/Unity-2022.3%2B-000000?style=for-the-badge&logo=unity&logoColor=white)
![Meta Quest 3](https://img.shields.io/badge/Meta%20Quest%203-Hand%20Tracking-0668E1?style=for-the-badge&logo=meta&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![VR Project](https://img.shields.io/badge/VR-Healthcare-red?style=for-the-badge)

[🌐 Version Française](#-version-française) • [🇬🇧 English Version](#-english-version)

</div>

---

## 🇫🇷 Version Française

### 📌 À propos
**HémiplégieVR** est un prototype d'application de réalité virtuelle médicale conçu pour accompagner les ergothérapeutes dans la rééducation de patients atteints d'hémiplégie.

L'application s'appuie sur le principe neuroscientifique de la **thérapie miroir** : elle capte les mouvements du bras sain du patient via les capteurs optiques du casque et les reproduit instantanément sur son bras virtuel affecté, stimulant ainsi la plasticité cérébrale sans nécessiter la manipulation de contrôleurs physiques.

> 🤝 **Partenariat de conception** : Projet réalisé au 4ème semestre du BUT Informatique Graphique en collaboration directe avec les ergothérapeutes de l'**Hôpital Émile-Roux (Le Puy-en-Velay)** pour la définition des besoins fonctionnels et cliniques.

---

### ✨ Approche Asymétrique Patient / Soignant

L'application propose une interface double parfaitement adaptée au contexte de soin :

- 🧘 **Côté Patient (Dans le casque)** :
  - Immersion dans un salon virtuel semi-réaliste aux tons pastels et apaisants.
  - Environnement totalement **No-UI** (épuration des interfaces visuelles) pour maximiser la concentration et l'engagement thérapeutique.
  - Interaction en **Hand Tracking** pur (sans contrôleurs).
- 🩺 **Côté Soignant (Sur l'écran PC)** :
  - Tableau de bord complet permettant au praticien de configurer et modifier la séance de manière dynamique en temps réel.

---

### 🎮 Activités Thérapeutiques

1. **Mouvement libre** : Travail de l'amplitude, de la symétrie et de la réappropriation du membre dans l'espace.
2. **Manipulation d'objets** : Exercices fonctionnels de préhension avec divers objets du quotidien (*livre, vase, brosse, tasse...*).

---

### 🎛️ Panneau de Contrôle du Praticien (Interface PC)

- 🔄 **Gestion de l'exercice** : Changement d'activité instantané en un clic.
- 🖐️ **Inversion de la latéralité** : Sélection dynamique de la main à tracker (gauche ou droite) selon le côté hémiplégique du patient.
- 🎨 **Inclusivité & Accessibilité** : Choix parmi 4 teintes de peau pour favoriser l'identification du patient à ses avatars de mains.
- 🦾 **Affichage adaptatif** : Option pour afficher ou masquer les avant-bras (pour se concentrer exclusivement sur les mains).
- 🎯 **Gestion de l'espace** : Recentrage instantané de la caméra (*"Réinitialiser le miroir"*) et bouton pour replacer les objets à leur position initiale.

---

### 💻 Matériel & Configuration

- **Casque** : Meta Quest 3 (*Hand Tracking optique complet activé*).
- **Connexion** : Meta Horizon Link (câble) ou Air Link.
- **Format** : Exécutable Standalone `.exe` fonctionnant sur tout PC Windows compatible VR.

#### Lancement de l'application :
1. Connectez le Meta Quest 3 au PC et activez le mode Link.
2. Assurez-vous que le **Hand Tracking** est activé dans les paramètres du casque.
3. Lancez le fichier `.exe` sur le PC.
4. L'interface soignant s'affiche sur le PC tandis que le flux VR immersif est envoyé au casque.

---

### 🔬 Statut du projet
Ce projet est un prototype fonctionnel développé dans un cadre universitaire. En raison des protocoles stricts d'analyses scientifiques et réglementaires indispensables au milieu clinique, l'application n'est pas déployée en conditions réelles à l'hôpital.

---

### 👥 Auteurs
- **Julien SAUGUES--EYRAUD** ([@JulienSauguesEyraud](https://github.com/JulienSauguesEyraud))
- **Lucas BRUN** ([@lucbr1](https://github.com/lucbr1))

---

<br/>

## 🇬🇧 English Version

### 📌 About
**HémiplégieVR** is a VR healthcare application prototype designed to assist occupational therapists in the rehabilitation of patients suffering from hemiplegia.

Based on the neuroscientific principle of **mirror therapy**, the software tracks the movements of the patient's healthy arm using optical sensors and mirrors them onto the virtual affected arm in real-time, stimulating neuroplasticity without physical controllers.

> 🤝 **Co-designed with healthcare professionals**: Developed during the 4th semester of the Computer Graphics Bachelor's degree (BUT Informatique Graphique) in direct collaboration with occupational therapists at **Émile-Roux Hospital (Le Puy-en-Velay)**.

---

### ✨ Asymmetric Patient / Therapist Approach

- 🧘 **Patient View (In VR Headset)**:
  - Calm, pastel-toned semi-realistic living room.
  - **No-UI environment** to enhance immersion and therapeutic focus.
  - Full **Hand Tracking** (controllerless experience).
- 🩺 **Therapist View (On PC Screen)**:
  - Comprehensive dashboard to configure and control the rehabilitation session in real time.

---

### 🎮 Therapeutic Exercises

1. **Free Movement**: Work on range of motion, symmetry, and limb spatial awareness.
2. **Object Manipulation**: Functional grasping exercises with daily objects (*book, vase, brush, cup...*).

---

### 🎛️ Therapist Control Panel (PC Interface)

- 🔄 **Exercise Control**: Switch activities seamlessly with one click.
- 🖐️ **Laterality Toggle**: Dynamically select which hand to track (left or right) based on the patient's condition.
- 🎨 **Inclusivity & Accessibility**: Choose between 4 skin tones to strengthen patient embodiment.
- 🦾 **Adaptive Rendering**: Toggle forearms visibility on/off.
- 🎯 **Space Management**: Instant camera recentering (*"Reset Mirror"*) and object position reset buttons.

---

### 💻 Setup & Requirements

- **Target Hardware**: Meta Quest 3 (*Full optical hand tracking enabled*).
- **Connection**: Meta Horizon Link (Cable or Air Link).
- **Distribution**: Standalone `.exe` for VR-ready Windows PCs.

---

### 👥 Authors
- **Julien SAUGUES--EYRAUD** ([@JulienSauguesEyraud](https://github.com/JulienSauguesEyraud))
- **Lucas BRUN** ([@lucbr1](https://github.com/lucbr1))

---
*Academic VR Rehabilitation Prototype.*
