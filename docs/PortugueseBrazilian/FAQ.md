# Perguntas e respostas

## Quais idiomas são suportados?

O mod possui traduções básicas para todos os idiomas atualmente suportados pelo jogo.

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
## Preciso começar um novo jogo?

Não. Este mod deve oferecer suporte completo a jogos salvos existentes.

## Ainda posso usar outros mods que adicionam pinturas?

Sim. Você não poderá sobrescrevê-las, mas pode copiá-las e editar as cópias.

## Onde os arquivos de pintura são salvos?

AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml

Por exemplo:
C:/Users/yourUserID/AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI

Você pode empacotar esses arquivos em um mod de pinturas.

## É possível usar valores maiores ou menores do que os sliders permitem?

Sim, mas o mod não oferece suporte direto a isso no momento. Pelo menos para cores, valores negativos quebram a cor e a deixam preta. Cores claras demais eventualmente também quebram a cor e começam a mostrar partes pretas cada vez maiores. Os parâmetros W de material podem causar grandes falhas de renderização, especialmente com certos valores XYZ de efeito.

## Posso alterar uma pintura integrada ou uma pintura fornecida por outros mods?

Não diretamente, mas você pode salvar uma cópia da pintura e editar a cópia.

## O que acontece se eu desinstalar este mod?

Deve ficar tudo bem. As pinturas que não existem mais parecerão pinturas padrão. Mas talvez você precise reatribuir as partes afetadas para usar pinturas diferentes, ou clicar com o botão direito na página de pinturas para limpar as pinturas para nenhuma pintura. Se você tinha uma pintura antiga de torso superior que não existe mais, mudar a pintura do Meka inteiro ou do torso inteiro pode deixar o torso superior preso na pintura padrão até que ele seja limpo com clique direito.

Fora isso, o jogo parece lidar bem o bastante com a desativação ou desinstalação do mod.

## Estou vendo grandes círculos brancos?

O intervalo normal de valores é entre 0 e 1. Qualquer outra coisa é experimental.

O principal causador da falha de renderização dos grandes círculos brancos é ter um valor W de material principal, secundário ou terciário grande demais, positivo ou negativo. Às vezes você pode contornar isso ajustando o slider XYZ de efeito correspondente (para efeito, X=principal, Y=secundário, Z=terciário), ou reduzindo a cor RGB da parte problemática (fazendo R+G+B ser menor). Reduzir o RGB pode deixá-la mais escura, mas parte do brilho pode ser restaurada usando um componente A negativo para essa cor.

Observação: os grandes círculos brancos podem aparecer apenas de certos ângulos, então use os botões de mover câmera (WASD por padrão, em teclados QWERTY) para examinar o Meka de vários ângulos. Tente passar um pouco do ponto em que os círculos somem, para ajudar a garantir que eles não reapareçam quando vistos exatamente pelo ângulo certo.

## Os sliders Supporter DLC não fazem nada?

Os efeitos só são visíveis se o DLC Supporter Upgrade tiver sido comprado e instalado. Além disso, para começar, garanta que as cores (XYZ=RGB) estejam, por exemplo, entre 0.5 e 1.0, e ajuste a partir daí. O valor W deve ser definido como algo diferente de none.

## O slider effect W não faz nada?

Até onde consigo perceber, esse valor não tem efeito.

## O que são os outros valores de pintura nos arquivos .yaml? Posso editá-los?

Ainda não investiguei todos eles e não tenho certeza do que fazem, se é que fazem algo. Atualmente, o mod só oferece suporte a cores/materiais principal/secundário/terciário e aos efeitos.

## Existe uma forma de excluir pinturas?

No momento, não por dentro do mod. Mas você pode excluir manualmente qualquer uma delas em AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml.

Isso deve ser seguro, exceto pelo fato de que as partes do seu Meka usarão a pintura padrão até que você reatribua pinturas especificamente às partes afetadas.

## Algo deu errado?

Pode haver informações adicionais em C:\Users\yourUserId\AppData\LocalLow\Brace Yourself Games\Phantom Brigade\Player.log.

Em particular, procure mensagens de log [LiveryGUI], ou mensagens de log sobre problemas em arquivos "LiveryGUI".