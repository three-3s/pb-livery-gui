# 问题与回答

## 支持哪些语言？

本模组为游戏当前支持的所有语言提供基础翻译。

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

## 我需要开始新游戏吗？

不需要。本模组应当完全支持现有存档。

## 我还能使用其他添加涂装的模组吗？

可以。你不能覆盖它们，但可以复制它们并编辑副本。

## 涂装文件保存在哪里？

AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml

例如：
C:/Users/yourUserID/AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI

你可以将这些文件打包成一个涂装模组。

## 是否可以使用比滑块允许范围更大或更小的值？

可以，但模组目前不支持直接这样做。至少对颜色来说，负值会“破坏”颜色并使其变黑。过亮的颜色最终也会“破坏”颜色，并开始显示越来越大的黑色区域。材质 W 参数可能触发一些严重的渲染故障，尤其是在某些效果 XYZ 值下。

## 我可以修改内置涂装或其他模组提供的涂装吗？

不能直接修改，但你可以保存该涂装的副本，然后编辑这个副本。

## 如果我卸载这个模组会怎样？

应该没有问题。不再存在的涂装会看起来像默认涂装。但你可能需要重新指定受影响部件使用其他涂装，或者在涂装页面中右键点击，将涂装清除为“无涂装”。如果你有一个旧的“上躯干”涂装已经不存在，改变整台机甲或整个躯干的涂装时，可能会让上躯干卡在“默认涂装”，直到它被清除（右键）。

除此之外，游戏似乎能比较好地处理禁用或卸载本模组的情况。

## 我看到了很大的白色圆圈？

数值的正常范围是 0 到 1。其他数值都属于实验性用法。

造成“巨大白色圆圈”渲染故障的主要原因，是主色、副色或第三色的材质 W 值过大，无论正负都可能发生。你有时可以通过调整对应的效果 XYZ 滑块来规避这个问题（效果中 X=主色，Y=副色，Z=第三色），或降低问题部件的 RGB 颜色（让 R+G+B 更小）。降低 RGB 可能会让颜色变暗，但可以通过给该颜色使用负的 A 分量来恢复一部分亮度。

注意：巨大白色圆圈可能只会从某些角度可见，所以请使用“移动摄像机”按键（QWERTY 键盘默认 WASD）从多个角度检查机甲。尝试比刚好消除白圈再多调一点，以帮助确保它们不会在某个刚好的角度再次出现。

## “Supporter DLC”滑块没有任何效果？

这些效果只有在已购买并安装 Supporter Upgrade DLC 时才可见。另外，开始调整时请先确保颜色（XYZ=RGB）大约在 0.5 到 1.0 之间，然后再从那里继续调整。W 值应设置为“none”之外的选项。

## “effect W”滑块没有任何效果？

就我所能判断，这个值没有效果。

## 涂装 .yaml 文件中的其他涂装值是什么？我可以编辑它们吗？

我还没有研究完所有这些值，也不确定它们是否有什么作用。模组目前只支持主色/副色/第三色的颜色和材质，以及效果参数。

## 有没有办法删除涂装？

目前不能在模组内部删除。但你可以手动从 AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml 删除任何涂装文件。

这应该是安全的，只是你的机甲部件会使用默认涂装，直到你专门为受影响的机甲部件重新指定涂装。

## 出问题了？

C:\Users\yourUserId\AppData\LocalLow\Brace Yourself Games\Phantom Brigade\Player.log 中可能有更多信息。

请特别查找 [LiveryGUI] 日志消息，或关于 “LiveryGUI” 文件问题的日志消息。