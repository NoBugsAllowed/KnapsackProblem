#include "Knapsack.h"
#include <iostream>

Knapsack::~Knapsack()
{
    //for (int i = 0; i < width; i++)
    //{
    //    delete used[i];
    //}
    //delete used;
}

Knapsack::Knapsack(int width, int height)
{
    this->width = width;
    this->height = height;

    used = new int*[width];
    for (int i = 0; i < width; i++)
    {
        used[i] = new int[height];
        for (int j = 0; j < height; j++)
        {
            used[i][j] = 0;
        }
    }
}

bool Knapsack::putElement(Element *r, int x, int y, bool diverted)
{
    if (r->onTable)
        return false;

    if (diverted)
    {
        if (isFreePlace(x, y, r->height, r->width))
        {
            markUsedTable(x, y, r->height, r->width, r->id);
            r->onTable = true;
            r->diverted = true;
            r->x = x;
            r->y = y;
            actValue += r->value;
            return true;
        }
        return false;
    }
    else
    {
        if (isFreePlace(x, y, r->width, r->height))
        {
            markUsedTable(x, y, r->width, r->height, r->id);
            r->onTable = true;
            r->diverted = false;
            r->x = x;
            r->y = y;
            actValue += r->value;
            return true;
        }
        return false;
    }
}

bool Knapsack::takeElement(Element *r)
{
    if (!r->onTable)
        return false;

    if (r->diverted)
    {
        clearMarkedRectangle(r->x, r->y, r->height, r->width);
        r->onTable = false;
        r->x = -1;
        r->y = -1;
        actValue -= r->value;
        return true;
    }
    else
    {
        clearMarkedRectangle(r->x, r->y, r->width, r->height);
        r->onTable = false;
        r->x = -1;
        r->y = -1;
        actValue -= r->value;
        return true;
    }
}

void Knapsack::drawSimpleKnapsack()
{
	std::cout << std::endl;
	for (int j = height - 1; j >= 0; j--)
	{
		for (int i = 0; i < width; i++)
		{
			std::cout << used[i][j];
		}
		std::cout << std::endl;
	}
}

bool Knapsack::isFreePlace(int x, int y, int width, int height)
{
    if (x + width > this->width)
        return false;

    if (y + height > this->height)
        return false;

    for (int i = x; i < x + width; i++)
    {
        for (int j = y; j < y + height; j++)
        {
            if (used[i][j] != 0)
                return false;
        }
    }
    return true;
}

bool Knapsack::markUsedTable(int x, int y, int width, int height, int markedNumber)
{
    if (x + width > this->width)
        return false;

    if (y + height > this->height)
        return false;

    for (int i = x; i < x + width; i++)
    {
        for (int j = y; j < y + height; j++)
        {
            used[i][j] = markedNumber;
        }
    }

    return true;
}

bool Knapsack::clearMarkedRectangle (int x, int y, int width, int height)
{
    return markUsedTable(x, y, width, height, 0);
}
