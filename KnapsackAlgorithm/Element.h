#pragma once

class Element
{
private:
	static int lastId;
public:
	bool diverted = false;
	bool onTable = false;
	int x = -1;
	int y = -1;
    int id;
	int height;
	int width;
	double value;
	bool used;

    Element();
	Element(int, int, double = 1.0, int = -1);
};
