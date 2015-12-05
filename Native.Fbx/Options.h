
#pragma once

using namespace System;
using namespace Fusion;
using namespace msclr::interop;
using namespace Fusion::Drivers::Graphics;
using namespace Fusion::Core::Shell;

/*
**	Options class :
*/
public ref class Options {
	private:
	string^ input;
	string^ output;
	string^ base;
	float	mergeTolerance;
	bool	wait;
	bool	anim;
	bool	geom;
	bool	report;
	bool	genmtrl;

	public:

	Options() {
		input			=	nullptr;
		output			=	nullptr;
		base			=	nullptr;
		mergeTolerance	=	0;
		wait			=	false;
		geom			=	false;
		anim			=	false;
	}

	[CommandLineParser::Name("in", "input FBX file")]
	[CommandLineParser::Required()]
	property string^ Input { 
		void set(string^ value) { input = value; }
		string ^get() { return input; }
	}

	[CommandLineParser::Name("out", "output scene file")]
	property string^ Output { 
		void set(string^ value) { output = value; }
		string ^get() { return output; }
	}

	[CommandLineParser::Name("merge", "merge tolerance (0.0 is default)")]
	property float MergeTolerance { 
		void set(float value) { mergeTolerance = value; } 
		float get() { return mergeTolerance; } 
	}

	[CommandLineParser::Name("base", "root directory")]
	property string ^BaseDirectory { 
		void set(string ^value) { base = value; } 
		string ^get() { return base; } 
	}

	[CommandLineParser::Name("anim", "bake and import animation tracks")]
	property bool ImportAnimation {
		void set(bool value) { anim = value; } 
		bool get() { return anim; } 
	}

	[CommandLineParser::Name("geom", "import geometry data.")]
	property bool ImportGeometry {
		void set(bool value) { geom = value; } 
		bool get() { return geom; } 
	}

	[CommandLineParser::Name("wait", "wait for user input after import")]
	property bool Wait {
		void set(bool value) { wait = value; } 
		bool get() { return wait; } 
	}

	[CommandLineParser::Name("report", "export html build report")]
	property bool Report {
		void set(bool value) { report = value; } 
		bool get() { return report; } 
	}

	[CommandLineParser::Name("genmtrl", "generate missing materials")]
	property bool GenerateMissingMaterials {
		void set(bool value) { genmtrl = value; } 
		bool get() { return genmtrl; } 
	}
};




