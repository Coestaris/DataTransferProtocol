#include "Plotter.h"

/*
File PLOTTER_CONFIG_FILE;
uint8_t PLOTTER_MOTOR_PINS[3][2] = 
	{ { PLOTTER_XSTEP, PLOTTER_XDIR },
	{ PLOTTER_YSTEP, PLOTTER_YDIR },
	{ PLOTTER_ZSTEP, PLOTTER_ZDIR } };

uint32_t PLOTTER_work = PLOTTER_WORK;
uint32_t PLOTTER_idle = PLOTTER_IDLE;
bool PLOTTER_pause = false;
bool PLOTTER_com = false;
uint32_t PLOTTER_DelayTime = false;
*/

void PLOTTER_INIT()
{
	if (SD.exists(CONFIGNAME))
	{
		PLOTTER_CONFIG_FILE = SD.open(CONFIGNAME, O_READ | O_WRITE);
		PLOTTER_MOTOR_PINS[0][0] = PLOTTER_CONFIG_FILE.read();
		PLOTTER_MOTOR_PINS[0][1] = PLOTTER_CONFIG_FILE.read();
		PLOTTER_MOTOR_PINS[1][0] = PLOTTER_CONFIG_FILE.read();
		PLOTTER_MOTOR_PINS[1][1] = PLOTTER_CONFIG_FILE.read();
		PLOTTER_MOTOR_PINS[2][0] = PLOTTER_CONFIG_FILE.read();
		PLOTTER_MOTOR_PINS[2][1] = PLOTTER_CONFIG_FILE.read();
		PLOTTER_work = (short)(PLOTTER_CONFIG_FILE.read() | (PLOTTER_CONFIG_FILE.read() << 8));
		PLOTTER_idle = (short)(PLOTTER_CONFIG_FILE.read() | (PLOTTER_CONFIG_FILE.read() << 8));
		PLOTTER_pause = PLOTTER_CONFIG_FILE.read() == 1;
		PLOTTER_com = PLOTTER_CONFIG_FILE.read() == 1;

	}
	else PLOTTER_ResetToDefault();
	pinMode(PLOTTER_PauseLed, OUTPUT);
	pinMode(PLOTTER_PauseCom, OUTPUT);
	for (int i = 0; i < 3; i++)
	{
		for (int count = 0; count < 2; count++)
		{
			pinMode(PLOTTER_MOTOR_PINS[i][count], OUTPUT);
		}
	}
	PLOTTER_DelayTime = 50;
	digitalWrite(PLOTTER_PauseLed, PLOTTER_pause);
	digitalWrite(PLOTTER_PauseCom, PLOTTER_com);
}

void PLOTTER_ResetToDefault()
{
	PLOTTER_CONFIG_FILE = SD.open(CONFIGNAME, O_READ | O_WRITE);
	byte* data = new byte[12]
	{
		PLOTTER_XSTEP,
		PLOTTER_XDIR,
		PLOTTER_YSTEP,
		PLOTTER_YDIR,
		PLOTTER_ZSTEP,
		PLOTTER_ZDIR,
		(byte)(PLOTTER_WORK & 0xFF),
		(byte)((PLOTTER_WORK >> 8) & 0xFF),
		(byte)(PLOTTER_IDLE & 0xFF),
		(byte)((PLOTTER_IDLE >> 8) & 0xFF),
		0,
		0
	};
	PLOTTER_CONFIG_FILE.write(data, 12);
	delete[] data;
}

void PLOTTER_moveForward(uint32_t sm)
{

}

void PLOTTER_moveBackward(uint32_t sm)
{

}

void PLOTTER_delayMicros(uint32_t wt)
{

}

void PLOTTER_MoveSM(int32_t x, int32_t y, int32_t z)
{

}
