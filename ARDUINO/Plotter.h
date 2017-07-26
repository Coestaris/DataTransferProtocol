#pragma once

#ifndef Plotter_h
#define Plotter_h

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#ifndef SDVarPlotter
extern SdFat SD;
#define SDVarPlotter
#endif

#include "Plotter_CORE.h"

#endif
