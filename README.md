# TerminalProgram
## *Вспомогательный софт*
Скрипт установщика написан с помощью Inno Setup Compiler 6.2.2

https://jrsoftware.org/isdl.php

## *Терминальная программа*
Проект написан с использованием .NET 7 и WPF.

Данная программа может выступать в роли *IP* и *SerialPort* клиента. Выбор типа клиента происходит в меню настроек.

Поддерживается два типовых режима работы:
1. Обмен данными по *стандартным* протоколам, которые поддерживает .NET.
2. Обмен данными по *специальным* протоколам.

# Краткое описание режимов работы
## *"Без протокола"*
В поле передачи пользователь пишет данные, которые нужно отправить. В поле приема находятся данные, которые прислал сервер или внешнее устройство.

	Поддерживаются протоколы: 
	- SerialPort (UART)
	- TCP

<p align="center">
  <img src="https://user-images.githubusercontent.com/86914394/220906424-6b901530-f951-4d5c-be10-871a10aea202.PNG" />
</p>

## *"Modbus"*
Пользователь может взаимодействовать с выбранными регистрами Modbus в соответствующих полях. История действий отображается в таблице.

	Поддерживаются протоколы: 
	- Modbus TCP
	- Modbus RTU

<p align="center">
  <img src="https://user-images.githubusercontent.com/86914394/227567508-d5e4754e-da06-4527-af9d-995bbb450129.PNG" />
</p>

## *"Http"*
В верхнем поле пользователь пишет http или https запрос. 

<p align="center">
  <img src="https://user-images.githubusercontent.com/86914394/220907786-e31e111b-855d-44b7-a443-99947973853f.PNG" />
</p>

# Система сохранений настроек

Все настройки можно сохранить в файле с расширением .json 

Перед подключением пользователь может выбрать из списка необходимый для работы файл с настройками или создать новый.

Настройки можно изменить в соответствующем пункте меню.

<p align="center">
  <img src="https://user-images.githubusercontent.com/86914394/220910112-a3dd74d7-ff94-4d0a-8ea6-e2a6e13968c8.PNG" />
  <img src="https://user-images.githubusercontent.com/86914394/220910147-737b9835-0225-4192-b44d-bac598e86745.PNG" />
</p>

# *Система версирования* Global.Major.Minor

*Global* - глобальная версия репозитория. До релиза это 0. Цифра меняется во время релиза и при именениях, затрагивающих значительную часть UI или внутренней логики.

*Major* - добавление нового функционала, крупные изменения.

*Minor* - исправление багов, мелкие добавления.
