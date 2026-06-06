# Fragen und Antworten

## Welche Sprachen werden unterstützt?

Der Mod enthält grundlegende Übersetzungen für alle Sprachen, die derzeit vom Spiel unterstützt werden.

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
## Muss ich ein neues Spiel beginnen?

Nein. Dieser Mod sollte vorhandene Spielstände vollständig unterstützen.

## Kann ich weiterhin andere Mods verwenden, die Lackierungen hinzufügen?

Ja. Du kannst sie nicht überschreiben, aber du kannst sie kopieren und die Kopien bearbeiten.

## Wo werden die Lackierungsdateien gespeichert?

AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml

Zum Beispiel:
C:/Users/yourUserID/AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI

Du kannst diese Dateien gern zu einem Lackierungsmod bündeln.

## Kann man größere oder kleinere Werte verwenden, als die Schieberegler erlauben?

Ja, aber der Mod unterstützt das derzeit nicht direkt. Zumindest bei Farben können negative Werte die Farbe brechen und schwarz machen. Zu helle Farben brechen irgendwann ebenfalls und zeigen zunehmend große schwarze Bereiche. Die Materialparameter W können größere Darstellungsfehler auslösen, besonders zusammen mit bestimmten Effekt-XYZ-Werten.

## Kann ich eine eingebaute Lackierung oder eine Lackierung aus anderen Mods ändern?

Nicht direkt, aber du kannst eine Kopie der Lackierung speichern und die Kopie bearbeiten.

## Was passiert, wenn ich diesen Mod deinstalliere?

Das sollte in Ordnung sein. Lackierungen, die nicht mehr existieren, sehen wie Standardlackierungen aus. Eventuell musst du betroffene Teile neu anderen Lackierungen zuweisen oder auf der Lackierungsseite mit Rechtsklick die Lackierungen auf keine Lackierung leeren. Wenn du eine alte Lackierung für den oberen Torso hattest, die nicht mehr existiert, kann eine Änderung der Lackierung des ganzen Mechs oder des ganzen Torsos dazu führen, dass der obere Torso auf Standardlackierung hängen bleibt, bis er geleert wird (Rechtsklick).

Abgesehen davon scheint das Spiel das Deaktivieren oder Deinstallieren des Mods gut genug zu handhaben.

## Ich sehe große weiße Kreise?

Der normale Wertebereich liegt zwischen 0 und 1. Alles andere ist experimentell.

Der wichtigste Auslöser für den Darstellungsfehler mit großen weißen Kreisen ist ein zu großer Material-W-Wert bei Haupt-, Neben- oder dritter Farbe, positiv oder negativ. Manchmal lässt sich das umgehen, indem du den entsprechenden Effekt-XYZ-Schieberegler anpasst (bei Effekten: X=Haupt, Y=Neben, Z=dritte), oder indem du die RGB-Farbe des problematischen Teils reduzierst (R+G+B kleiner machen). Weniger RGB kann die Farbe dunkler machen, aber ein Teil der Helligkeit kann durch eine negative A-Komponente dieser Farbe zurückgeholt werden.

Hinweis: Die großen weißen Kreise sind manchmal nur aus bestimmten Winkeln sichtbar. Verwende deshalb die Tasten für Kamera bewegen (standardmäßig WASD auf QWERTY-Tastaturen), um den Mech aus mehreren Winkeln zu prüfen. Versuche, die Kreise ein wenig stärker zu beseitigen als gerade nötig, damit sie nicht bei genau passendem Blickwinkel wieder auftauchen.

## Die Supporter-DLC-Schieberegler machen nichts?

Die Effekte sind nur sichtbar, wenn der Supporter Upgrade DLC gekauft und installiert ist. Stelle zu Beginn außerdem sicher, dass die Farben (XYZ=RGB) zum Beispiel zwischen 0.5 und 1.0 liegen, und passe sie von dort aus an. Der W-Wert sollte auf etwas anderes als none gesetzt sein.

## Der Schieberegler effect W macht nichts?

Soweit ich beurteilen kann, hat dieser Wert keine Wirkung.

## Was sind die anderen Lackierungswerte in den .yaml-Dateien? Kann ich sie bearbeiten?

Ich habe noch nicht alle untersucht und bin nicht sicher, was sie tun, falls sie überhaupt etwas tun. Der Mod unterstützt derzeit nur Haupt-/Neben-/dritte Farben und Materialien sowie die Effekte.

## Gibt es eine Möglichkeit, Lackierungen zu löschen?

Im Moment nicht direkt im Mod. Du kannst sie aber manuell aus AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml löschen.

Das sollte sicher sein, abgesehen davon, dass deine Mech-Teile die Standardlackierung verwenden, bis du den betroffenen Mech-Teilen ausdrücklich wieder Lackierungen zuweist.

## Etwas ist schiefgelaufen?

Möglicherweise gibt es weitere Informationen in C:\Users\yourUserId\AppData\LocalLow\Brace Yourself Games\Phantom Brigade\Player.log.

Suche insbesondere nach [LiveryGUI]-Logmeldungen oder nach Logmeldungen über Probleme in "LiveryGUI"-Dateien.