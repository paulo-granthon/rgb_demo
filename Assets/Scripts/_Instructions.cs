//==		==		==		==		==		==		==		==		==		==		==		==		==		==		==		==		==		==		==
// ESTE DOCUMENTO NÃO É UM SCRIPT E SIM UM GUIA PARA A EXECUÇÃO DO PROJETO, 
// INFORMAÇÕES GUIA SERÃO ARMAZENADAS AQUI PARA FACILITAR O MEU TRABALHO COMO PROGRAMADOR
//==		==		==		==		==		==		==		==		==		==		==		==		==		==		==		==		==		==		==
// quando o duelo começar, o game object da "mesa", ira ser ativado, resetando todos os outros objetos que compõe o duelo
// o deck de ambos os players serão atualizados:
//		deck do jogador: será baseado nas informações armazenadas pelo deck editor (0% progresso)
//		deck do inimigo: será armazenado em uma ficha de scriptable object q será armazenado dentro de um script de IA generico,
//						 o script de ia será um holder pras informações q vão conter no scriptable object, tais como comportamento
//						 no game world, deck, estilo de jogo, etc 
// inicio do duelo: gameobject "table" será ativado, Duel manager irá iniciar um metódo que resetará os objetos da mesa, deck e etc
//					alem de determinar o jogador que começa, informações serão requeridas pelo script de IA que inicia o duelo
//					essas informações serão: (ScriptableObject "deck, ia, etc, sobre o inimigo", "algo que diga como o duelo irá começar") 
//					e talvez algumas coisas a mais 
// durante o duelo: DuelManager irá se encarregar de tudo, ele é o script que interpreta as ações do jogador e transmite para o duelo
//					alem de conter algumas informações extremamente necessarias para o mesmo e ser encarregado das passagens de turno
//					e frações de turnos ("phases")
// fim do duelo:	informações como o "drop" em Card / Materia / Money, serão armazenadas tambem no ScriptableObject IA do inimigo
//					a mesa será limpa, resetanto objetos nela, pontos de vida, Graveyard e decks, logo após, haverá um comando que finaliza
//					o duelo, desativando a mesa pelo DuelManager 
// em resumo: o duelo será chamado pela IA, neste caso, em função de testes, irei colocar um Input que chama o duelo 
//					
//					outras informações importantes:
//
//	o GameObject Player será desativado assim que o Duel manager iniciar o jogo, para evitar qualquer tipo de interação indesejada
//	no game world durante o duelo 