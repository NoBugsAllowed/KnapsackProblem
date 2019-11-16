#pragma once
#include "Element.h"

class Knapsack
{
public:
    int width;
    int height;
    int** used;
    int actValue = 0;

    Knapsack() {}
    Knapsack(int, int);
    ~Knapsack();
    bool putElement(Element *r, int, int, bool = false);
    bool takeElement(Element *r);
	void drawSimpleKnapsack();

private:
    bool isFreePlace(int, int, int, int);
    bool markUsedTable(int, int, int, int, int);
    bool clearMarkedRectangle(int, int, int, int);
};
