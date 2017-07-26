#pragma once

// Plotter.h

#ifndef _plotter_core_h
#define _plotter_core_h

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include <EEPROM.h>

#define PLOTTER_PauseLed 8
#define PLOTTER_PauseCom 9
#define PLOTTER_ButtonStop A2
#define PLOTTER_PauseLed 8
#define PLOTTER_PauseCom 9
#define PLOTTER_ButtonStop A2
#define PLOTTER_ButtonPause A1
#define PLOTTER_Analog A0

#define PLOTTER_XDIR 4
#define PLOTTER_YDIR 2
#define PLOTTER_ZDIR 6
#define PLOTTER_XSTEP 5
#define PLOTTER_YSTEP 3
#define PLOTTER_ZSTEP 7
#define PLOTTER_WORK 50 
#define PLOTTER_IDLE 40

#include "SDFAT\SdFat.h";
#include "SPI.h"

#define CONFIGNAME "/config"

extern File PLOTTER_CONFIG_FILE;
extern uint8_t PLOTTER_MOTOR_PINS[3][2] = { { PLOTTER_XSTEP, PLOTTER_XDIR },
											{ PLOTTER_YSTEP, PLOTTER_YDIR },
											{ PLOTTER_ZSTEP, PLOTTER_ZDIR } };

extern uint32_t PLOTTER_work = PLOTTER_WORK;
extern uint32_t PLOTTER_idle = PLOTTER_IDLE;
extern bool PLOTTER_pause = false;
extern bool PLOTTER_com = false;
extern uint32_t PLOTTER_DelayTime = false;

void PLOTTER_INIT();

void PLOTTER_ResetToDefault();

void PLOTTER_moveForward(uint32_t sm);

void PLOTTER_moveBackward(uint32_t sm);

void PLOTTER_delayMicros(uint32_t wt);

void PLOTTER_MoveSM(int32_t x, int32_t y, int32_t z);
#endif