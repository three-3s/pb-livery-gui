# Вопросы и ответы

## Какие языки поддерживаются?

Мод содержит базовые переводы для всех языков, которые сейчас поддерживает игра.

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
## Нужно ли начинать новую игру?

Нет. Этот мод должен полностью поддерживать существующие сохранения.

## Можно ли продолжать использовать другие моды, добавляющие окраски?

Да. Вы не сможете перезаписывать их окраски, но сможете копировать их и редактировать копии.

## Где сохраняются файлы окраски?

AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml

Например:
C:/Users/yourUserID/AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI

Эти файлы можно упаковать в отдельный мод с окрасками.

## Можно ли использовать значения больше или меньше, чем позволяют слайдеры?

Да, но сейчас мод напрямую этого не поддерживает. По крайней мере для цветов отрицательные значения ломают цвет и делают его черным. Слишком яркие цвета тоже рано или поздно ломаются и начинают показывать все более крупные черные области. Параметры материала W могут вызывать серьезные ошибки рендеринга, особенно с некоторыми значениями эффектов XYZ.

## Можно ли изменить встроенную окраску или окраску из другого мода?

Не напрямую, но можно сохранить копию окраски и редактировать эту копию.

## Что будет, если удалить этот мод?

Все должно быть в порядке. Окраски, которых больше не существует, будут выглядеть как стандартные окраски. Но может потребоваться заново назначить затронутым деталям другие окраски или щелкнуть правой кнопкой мыши на странице окраски, чтобы очистить окраску до нет окраски. Если у вас была старая окраска верхней части торса, которой больше не существует, изменение окраски всего меха или всего торса может оставить верхнюю часть торса застрявшей на стандартной окраске, пока она не будет очищена правым щелчком.

Кроме этого, игра, похоже, достаточно хорошо справляется с отключением или удалением мода.

## Я вижу большие белые круги?

Нормальный диапазон значений находится между 0 и 1. Все остальное является экспериментальным.

Главная причина ошибки рендеринга с большими белыми кругами — слишком большое значение W материала для основного, дополнительного или третьего цвета, как положительное, так и отрицательное. Иногда это можно обойти, изменив соответствующий слайдер эффекта XYZ (для эффектов: X=основной, Y=дополнительный, Z=третий), или уменьшив RGB-цвет проблемной детали (сделав R+G+B меньше). Уменьшение RGB может сделать ее темнее, но часть яркости можно вернуть, используя отрицательную компоненту A для этого цвета.

Примечание: большие белые круги могут быть видны только под некоторыми углами, поэтому используйте клавиши движения камеры (по умолчанию WASD на клавиатурах QWERTY), чтобы осмотреть меха с разных сторон. Попробуйте немного превысить точку исчезновения белых кругов, чтобы они не появлялись снова при взгляде точно под нужным углом.

## Слайдеры Supporter DLC ничего не делают?

Эффекты видны только если DLC Supporter Upgrade куплен и установлен. Кроме того, для начала убедитесь, что цвета (XYZ=RGB) находятся, например, между 0.5 и 1.0, и настраивайте их оттуда. Значение W должно быть чем-то отличным от none.

## Слайдер effect W ничего не делает?

Насколько я могу судить, это значение ни на что не влияет.

## Что означают другие значения окраски в файлах .yaml? Можно ли их редактировать?

Я еще не изучил их все и не уверен, что они делают, если вообще что-то делают. Сейчас мод поддерживает только основные/дополнительные/третьи цвета и материалы, а также эффекты.

## Есть ли способ удалить окраски?

На данный момент не из самого мода. Но вы можете вручную удалить любые из них из AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml.

Это должно быть безопасно, за исключением того, что детали ваших мехов будут использовать стандартную окраску, пока вы явно не назначите новые окраски затронутым деталям.

## Что-то пошло не так?

Дополнительная информация может быть в C:\Users\yourUserId\AppData\LocalLow\Brace Yourself Games\Phantom Brigade\Player.log.

В частности, ищите сообщения журнала [LiveryGUI] или сообщения о проблемах в файлах "LiveryGUI".