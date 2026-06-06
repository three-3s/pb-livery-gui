# 質問と回答

## どの言語に対応していますか？

このModには、現在ゲームが対応しているすべての言語向けの基本翻訳があります。

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
## 新しいゲームを始める必要がありますか？

いいえ。このModは既存のセーブデータを完全にサポートするはずです。

## 塗装を追加する他のModも使えますか？

はい。上書きはできませんが、コピーを作成してそのコピーを編集できます。

## 塗装ファイルはどこに保存されますか？

AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml

例：
C:/Users/yourUserID/AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI

これらのファイルを塗装Modとしてまとめてもかまいません。

## スライダーの範囲より大きい値や小さい値を使えますか？

可能ですが、現在このModは直接対応していません。少なくとも色については、負の値は色を壊して黒くします。明るすぎる色も最終的には色が壊れ、黒い部分がだんだん大きく表示されます。材質のWパラメーターは、特に一部のエフェクトXYZ値と組み合わさると、大きな表示不具合を起こすことがあります。

## 標準の塗装や他のModが提供する塗装を変更できますか？

直接は変更できません。ただし、その塗装のコピーを保存し、そのコピーを編集できます。

## このModをアンインストールするとどうなりますか？

おそらく問題ありません。存在しなくなった塗装は標準塗装のように見えます。ただし、影響を受けた部品に別の塗装を再割り当てするか、塗装ページで右クリックして塗装を「塗装なし」に消去する必要があるかもしれません。古い「上部胴体」塗装が存在しなくなった場合、メカ全体や胴体全体の塗装を変更しても、上部胴体が消去されるまで「標準塗装」に固定されたようになることがあります（右クリックで消去）。

それ以外については、ゲームはModの無効化やアンインストールを十分うまく処理しているようです。

## 大きな白い円が見えます。

通常の値の範囲は0から1です。それ以外は実験的な値です。

「大きな白い円」の表示不具合の主な原因は、メイン、サブ、第3のいずれかの材質W値が大きすぎることです。正の値でも負の値でも発生します。対応するエフェクトXYZスライダーを調整することで回避できる場合があります（エフェクトでは X=メイン、Y=サブ、Z=第3）。または、問題の部品のRGB色を下げてください（R+G+Bを小さくする）。RGBを下げると暗くなる場合がありますが、その色のA成分を負にすると明るさをある程度戻せます。

注意：大きな白い円は特定の角度からしか見えないことがあります。メカを複数の角度から確認するために、「カメラ移動」キー（QWERTYキーボードでは既定でWASD）を使ってください。白い円がぎりぎり消えるところより少し余裕を持って調整すると、ちょうど特定の角度で再発するのを避けやすくなります。

## 「Supporter DLC」スライダーが何もしません。

この効果は、Supporter Upgrade DLCを購入してインストールしている場合にのみ表示されます。また、最初は色（XYZ=RGB）をたとえば0.5から1.0の間にして、そこから調整してください。W値は none 以外に設定する必要があります。

## 「effect W」スライダーが何もしません。

私が確認できる限り、この値には効果がありません。

## 塗装の.yamlファイルにある他の値は何ですか？編集できますか？

まだすべては調べておらず、それらが何をするのか、あるいは何かをするのかも確かではありません。このModが現在対応しているのは、メイン/サブ/第3の色と材質、およびエフェクトだけです。

## 塗装を削除する方法はありますか？

現時点ではMod内からは削除できません。ただし、AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml から手動で削除できます。

これは安全なはずです。ただし、影響を受けたメカ部品にあらためて塗装を割り当てるまで、その部品は標準塗装を使用します。

## 何か問題が起きましたか？

C:\Users\yourUserId\AppData\LocalLow\Brace Yourself Games\Phantom Brigade\Player.log に追加情報があるかもしれません。

特に [LiveryGUI] のログメッセージ、または “LiveryGUI” ファイルの問題に関するログメッセージを探してください。