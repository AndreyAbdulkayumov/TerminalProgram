# TerminalProgram

## *Статьи на Хабр*

[Кроссплатформенный терминал Modbus TCP / RTU / ASCII с открытым исходным кодом: Часть 2](https://habr.com/ru/articles/854824/)

[Терминал Modbus TCP / RTU / ASCII с открытым исходным кодом: Часть 1](https://habr.com/ru/articles/795387/)

## *Терминальная программа*
Начиная с версии 3.0.0 в проекте используется Avalonia UI, в более ранних версиях WPF.

Платформа:
- .NET Framework до версии 1.9.1 включительно.
- .NET 7 начиная с версии 1.10.0.
- .NET 8 начиная с версии 2.3.0.
- .NET 9 начиная с версии 3.0.0.

Приложение может выступать в роли *IP* и *SerialPort* клиента. Выбор типа клиента происходит в меню настроек.

Поддерживается два типовых режима работы:
1. Обмен данными по *стандартным* протоколам, которые поддерживает .NET.
2. Обмен данными по *специальным* протоколам.

Приложение поддерживает ***Темную*** и ***Светлую*** тему оформления.

# Режимы работы

Подробнее о приложении можно узнать из статей на Хабр. Там есть более подробное описание и инструкция. 

## *"Без протокола"*
В поле передачи пользователь пишет данные, которые нужно отправить. В поле приема находятся данные, которые прислал сервер или внешнее устройство. 

	Поддерживаются протоколы: 
	- UART
	- TCP

<p align="center">
  <img src="https://github.com/user-attachments/assets/abafe38b-fe23-45d4-87bf-ad9c16c2453a"/>
</p>

<p align="center">
  <img src="https://github.com/user-attachments/assets/730c45ef-106a-4a69-85fe-75880ec3d3f3"/>
</p>

## *"Modbus"*
Пользователь может взаимодействовать с выбранными регистрами Modbus, используя соответствующие элементы интерфейса. Для дополнительной расшифровки транзакции существует раздел с представлениями.

	Поддерживаются протоколы: 
	- Modbus TCP
	- Modbus RTU
 	- Modbus ASCII
  	- Modbus RTU over TCP
 	- Modbus ASCII over TCP

<p align="center">
  <img src="https://github.com/user-attachments/assets/00f85b38-ac78-453d-b3d6-b36a68c26afd"/>
</p>

<p align="center">
  <img src="https://github.com/user-attachments/assets/20cb7161-e1dc-43aa-b1c9-6eabea093ccb"/>
</p>

# *Вспомогательный софт*
GUI Framework - [Avalonia UI](https://avaloniaui.net/)

Для упрощения работы с паттерном MVVM использован [ReactiveUI](https://www.reactiveui.net/)

Для тестирования используется [xUnit](https://xunit.net/)

Скрипт установщика написан с помощью [Inno Setup Compiler](https://jrsoftware.org/isdl.php)

Иконки приложения [Material.Icons.Avalonia](https://github.com/AvaloniaUtils/Material.Icons.Avalonia/)

# *Система версирования* Global.Major.Minor

*Global* - глобальная версия репозитория. До релиза это 0. Цифра меняется во время релиза и при именениях, затрагивающих значительную часть UI или внутренней логики.

*Major* - добавление нового функционала, крупные изменения.

*Minor* - исправление багов, мелкие добавления.
