# Questions et réponses

## Quelles langues sont prises en charge ?

Le mod contient des traductions de base pour toutes les langues actuellement prises en charge par le jeu.

<table>
  <tr>
    <td>English</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/README.md">README</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/FAQ.md">FAQ</a></td>
  </tr>
  <tr>
    <td>简体中文</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/ChineseSimplified/README.md">说明文档</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/ChineseSimplified/FAQ.md">常见问题</a></td>
  </tr>
  <tr>
    <td>繁體中文</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/ChineseTraditional/README.md">說明文件</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/ChineseTraditional/FAQ.md">常見問題</a></td>
  </tr>
  <tr>
    <td>Français</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/French/README.md">Lisez-moi</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/French/FAQ.md">FAQ</a></td>
  </tr>
  <tr>
    <td>Deutsch</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/German/README.md">Liesmich</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/German/FAQ.md">Häufige Fragen</a></td>
  </tr>
  <tr>
    <td>Italiano</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Italian/README.md">Leggimi</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Italian/FAQ.md">Domande frequenti</a></td>
  </tr>
  <tr>
    <td>日本語</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Japanese/README.md">はじめに</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Japanese/FAQ.md">よくある質問</a></td>
  </tr>
  <tr>
    <td>한국어</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Korean/README.md">읽어보기</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Korean/FAQ.md">자주 묻는 질문</a></td>
  </tr>
  <tr>
    <td>Português Brasileiro</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/PortugueseBrazilian/README.md">Leia-me</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/PortugueseBrazilian/FAQ.md">Perguntas frequentes</a></td>
  </tr>
  <tr>
    <td>Русский</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Russian/README.md">Прочитайте</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Russian/FAQ.md">Часто задаваемые вопросы</a></td>
  </tr>
  <tr>
    <td>Español</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Spanish/README.md">Léeme</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Spanish/FAQ.md">Preguntas frecuentes</a></td>
  </tr>
</table>
## Dois-je commencer une nouvelle partie ?

Non. Ce mod devrait prendre entièrement en charge les sauvegardes existantes.

## Puis-je encore utiliser d'autres mods qui ajoutent des livrées ?

Oui. Vous ne pourrez pas les écraser, mais vous pouvez les copier et modifier les copies.

## Où les fichiers de livrée sont-ils enregistrés ?

AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml

Par exemple :
C:/Users/yourUserID/AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI

Vous pouvez regrouper ces fichiers dans un mod de livrées.

## Est-il possible d'utiliser des valeurs plus grandes ou plus petites que celles autorisées par les curseurs ?

Oui, mais le mod ne le prend pas en charge directement pour le moment. Pour les couleurs au moins, les valeurs négatives cassent la couleur et la rendent noire. Les couleurs trop lumineuses finissent aussi par casser la couleur et commencent à afficher des portions noires de plus en plus grandes. Les paramètres W des matériaux peuvent provoquer de gros problèmes de rendu, surtout avec certaines valeurs XYZ des effets.

## Puis-je modifier une livrée intégrée ou une livrée fournie par un autre mod ?

Pas directement, mais vous pouvez enregistrer une copie de la livrée et modifier cette copie.

## Que se passe-t-il si je désinstalle ce mod ?

Cela devrait aller. Les livrées qui n'existent plus ressembleront à des livrées par défaut. Vous devrez peut-être réassigner les pièces concernées à d'autres livrées, ou faire un clic droit dans la page des livrées pour les effacer et les remettre sur aucune livrée. Si vous aviez une ancienne livrée de torse supérieur qui n'existe plus, changer la livrée du Mech entier ou du torse entier peut laisser le torse supérieur bloqué sur la livrée par défaut jusqu'à ce qu'il soit effacé par clic droit.

À part cela, le jeu semble gérer assez correctement la désactivation ou la désinstallation du mod.

## Je vois de grands cercles blancs ?

La plage normale des valeurs est entre 0 et 1. Tout le reste est expérimental.

La cause principale du problème de rendu des grands cercles blancs est une valeur W de matériau principale, secondaire ou troisième trop élevée, positive ou négative. Vous pouvez parfois contourner le problème en ajustant le curseur XYZ de l'effet correspondant (pour les effets, X=principal, Y=secondaire, Z=troisième), ou en réduisant la couleur RGB de la pièce concernée (rendre R+G+B plus petit). Réduire le RGB peut assombrir la couleur, mais une partie de la luminosité peut être récupérée avec une composante A négative pour cette couleur.

Remarque : les grands cercles blancs peuvent être visibles seulement sous certains angles. Utilisez donc les touches de déplacement de la caméra (WASD par défaut sur les claviers QWERTY) pour examiner le Mech sous plusieurs angles. Essayez de dépasser un peu le point où les cercles disparaissent, afin d'éviter qu'ils ne réapparaissent sous un angle précis.

## Les curseurs Supporter DLC ne font rien ?

Les effets ne sont visibles que si le DLC Supporter Upgrade est acheté et installé. Pour commencer, assurez-vous aussi que les couleurs (XYZ=RGB) sont, par exemple, entre 0.5 et 1.0, puis ajustez-les à partir de là. La valeur W doit être réglée sur autre chose que none.

## Le curseur effect W ne fait rien ?

D'après ce que je peux voir, cette valeur n'a aucun effet.

## Que sont les autres valeurs de livrée dans les fichiers .yaml ? Puis-je les modifier ?

Je ne les ai pas encore toutes étudiées et je ne suis pas sûr de ce qu'elles font, si elles font quelque chose. Le mod ne prend actuellement en charge que les couleurs et matériaux principal/secondaire/troisième, ainsi que les effets.

## Existe-t-il un moyen de supprimer des livrées ?

Pas depuis le mod pour le moment. Mais vous pouvez supprimer manuellement les fichiers voulus dans AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml.

Cela devrait être sûr, sauf que vos pièces de Mech utiliseront la livrée par défaut jusqu à ce que vous réassigniez explicitement des livrées aux pièces concernées.

## Quelque chose s est mal passé ?

Il peut y avoir des informations supplémentaires dans C:\Users\yourUserId\AppData\LocalLow\Brace Yourself Games\Phantom Brigade\Player.log.

Cherchez en particulier les messages de journal [LiveryGUI], ou les messages à propos de problèmes dans des fichiers "LiveryGUI".
