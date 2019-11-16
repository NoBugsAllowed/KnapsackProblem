#include "Element.h"

int Element::lastId = 1;

Element::Element() 
{
	this->id = Element::lastId++;
}

Element::Element(int width, int height, double value, int id)
{
	this->height = height;
	this->width = width;
	this->value = value;
	this->used = false;
	if (id < 0)
		this->id = Element::lastId++;
	else
		this->id = id;
}
