#pragma once
#include "Knapsack.h"
#include <fstream>

class Algorithm
{
private:
    Knapsack knapsack;
    Element* elements;
    int elementsCount;
    double actValue = 0;
    bool* used;
    int elementsOnKnapsack = 0;
public:
    Element* bestElements;
    double bestValue = 0;
	int callsCount = 0;

    Algorithm(Knapsack&,Element*,int);
    ~Algorithm();

    bool compute(int = 0);
};
