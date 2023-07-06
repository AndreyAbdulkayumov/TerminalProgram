# TerminalProgram
## *Вспомогательный софт*
Скрипт установщика написан с помощью Inno Setup Compiler 6.2.2

https://jrsoftware.org/isdl.php

Для упрощения работы с паттерном MVVM использован ReactiveUI

https://www.reactiveui.net/

## *Терминальная программа*
Проект написан с использованием WPF.

Платформа:
- .NET Framework до версии 1.9.1 включительно.
- .NET 7 начиная с версии 1.10.0.

Данная программа может выступать в роли *IP* и *SerialPort* клиента. Выбор типа клиента происходит в меню настроек.

Поддерживается два типовых режима работы:
1. Обмен данными по *стандартным* протоколам, которые поддерживает .NET.
2. Обмен данными по *специальным* протоколам.

Приложение поддерживает следующие темы оформления:
1. Темная.
2. Светлая.

# Краткое описание режимов работы
## *"Без протокола"*
В поле передачи пользователь пишет данные, которые нужно отправить. В поле приема находятся данные, которые прислал сервер или внешнее устройство.

	Поддерживаются протоколы: 
	- SerialPort (UART)
	- TCP

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/89350536-c9a7-4c56-b453-7645ce9696cd" />
</p>

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/817c8071-438a-463e-ac92-250eb52f6b5a" />
</p>

## *"Modbus"*
Пользователь может взаимодействовать с выбранными регистрами Modbus в соответствующих полях. История действий отображается в таблице.

	Поддерживаются протоколы: 
	- Modbus TCP
	- Modbus RTU

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/96b47588-22c9-4b17-a8d2-54109071e306" />
</p>

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/7b8d0533-9483-4ce5-a9af-a25207a15f91" />
</p>

## *"Http"*
В верхнем поле пользователь пишет http или https запрос. 

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/30b7b3d7-ab76-4793-ac34-3f2fb9036720" />
</p>

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/90967104-9518-41cf-acdf-f7e668d54765" />
</p>

# Система сохранений настроек

Все настройки можно сохранить в файле с расширением .json 

Перед подключением пользователь может выбрать из списка необходимый для работы файл с настройками или создать новый.

Настройки можно изменить в соответствующем пункте меню.

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/4eb62085-2d01-4dc4-8369-d5b4736a9646" />
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/f6c5a88b-2aa2-4a6a-912d-1d9c33c9d3e5" />
</p>

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/e794d857-cebb-4337-86da-e0e587024c76" />
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/a54abbfb-7634-4c89-9b55-21d9accd2062" />
</p>

# *Система версирования* Global.Major.Minor

*Global* - глобальная версия репозитория. До релиза это 0. Цифра меняется во время релиза и при именениях, затрагивающих значительную часть UI или внутренней логики.

*Major* - добавление нового функционала, крупные изменения.

*Minor* - исправление багов, мелкие добавления.
