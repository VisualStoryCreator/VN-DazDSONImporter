using System.Collections.Generic;
using System.Linq;

public class Outfit {
	public static List<Outfit> Outfits = new List<Outfit> {
		new Outfit("Bandeau Bikini", new List<string> {
			"bandeau-bikini-top",
			"bandeau-bikini-bottoms" }),
		new Outfit("Breakfast In Bed Pajamas", new List<string> {
			"breakfast-in-bed-tank",
			"breakfast-in-bed-shorts" }),
		new Outfit("Relaxed Sunday", new List<string> {
			"relaxed-sunday-tank",
			"relaxed-sunday-shorts",
			"relaxed-sunday-shoes" }),
		new Outfit("Summer Dress Set", new List<string> {
			"summer-dress-dress",
			"summer-dress-shoes" }),
		new Outfit("Upscale Shopper", new List<string> {
			"upscale-shopper-blazer",
			"upscale-shopper-blouse",
			"upscale-shopper-pants",
			"upscale-shopper-heels" }),
	};

	public bool IsMatch(FigureFacade[] figures) {
		var isMatch = figures.Select(figure => figure.Definition.Name)
			.SequenceEqual(Figures);
		return isMatch;
	}

	public string Label { get; }
	public List<string> Figures { get; }

	public Outfit(string label, List<string> figures) {
		Label = label;
		Figures = figures;
	}
}
