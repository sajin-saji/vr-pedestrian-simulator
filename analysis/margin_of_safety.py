"""
Margin of safety for a pedestrian crossing a road (Table I of the paper).

    car arrival time  = distance car -> player / car speed
    time to cross     = road width / walking speed
    margin of safety  = car arrival time - time to cross

A positive margin means the player is across before the car arrives.
A negative margin means insufficient time in this simplified model.

The paper fixes the road width at 8 m. It does not print the car distance,
but every value in Table I fits a distance of 80 m, so that is used here.

Run:  python analysis/margin_of_safety.py
Out:  analysis/results/margin_of_safety.csv
      analysis/results/margin_of_safety_heatmap.png
"""

from pathlib import Path
import csv

ROAD_WIDTH_M = 8.0
CAR_DISTANCE_M = 80.0

CAR_SPEEDS = [8, 10, 12, 15, 22, 28]            # m/s
WALK_SPEEDS = [1.0, 1.4, 2.6, 3.5, 4.8]         # m/s: slow walk ... run

# Values exactly as printed in the paper, used to check the calculation.
PAPER_TABLE = {
    8:  [2.00, 4.29, 6.92, 7.71, 8.33],
    10: [0.00, 2.29, 4.92, 5.71, 6.33],
    12: [-1.33, 0.96, 3.59, 4.38, 5.00],
    15: [-2.67, -0.38, 2.25, 3.04, 3.66],
    22: [-4.36, -2.07, 0.55, 1.34, 1.97],
    28: [-5.14, -2.85, -0.22, 0.59, 1.19],
}

OUT = Path(__file__).parent / "results"


def margin(car_speed, walk_speed, distance=CAR_DISTANCE_M, width=ROAD_WIDTH_M):
    return distance / car_speed - width / walk_speed


def main():
    OUT.mkdir(exist_ok=True)
    table = {c: [margin(c, w) for w in WALK_SPEEDS] for c in CAR_SPEEDS}

    # CSV
    with open(OUT / "margin_of_safety.csv", "w", newline="") as f:
        wr = csv.writer(f)
        wr.writerow(["car_speed_mps"] + [f"walk_{w}_mps" for w in WALK_SPEEDS])
        for c in CAR_SPEEDS:
            wr.writerow([c] + [f"{m:.2f}" for m in table[c]])

    # Print and compare with the paper
    print(f"Margin of safety (s), road {ROAD_WIDTH_M:g} m, car distance {CAR_DISTANCE_M:g} m")
    print("car\\walk " + "".join(f"{w:>8}" for w in WALK_SPEEDS))
    max_diff = 0.0
    for c in CAR_SPEEDS:
        print(f"{c:>8} " + "".join(f"{m:>8.2f}" for m in table[c]))
        for m, p in zip(table[c], PAPER_TABLE[c]):
            max_diff = max(max_diff, abs(round(m, 2) - p))
    print(f"\nLargest difference to the paper's Table I: {max_diff:.2f} s (rounding)")

    # Heatmap
    try:
        import matplotlib.pyplot as plt
        from matplotlib.colors import TwoSlopeNorm
    except ImportError:
        print("matplotlib not installed, skipping the plot")
        return

    data = [table[c] for c in CAR_SPEEDS]
    fig, ax = plt.subplots(figsize=(7, 4.5))
    im = ax.imshow(data, cmap="RdYlGn", norm=TwoSlopeNorm(vmin=-6, vcenter=0, vmax=9))
    ax.set_xticks(range(len(WALK_SPEEDS)), [f"{w:g}" for w in WALK_SPEEDS])
    ax.set_yticks(range(len(CAR_SPEEDS)), [f"{c}" for c in CAR_SPEEDS])
    ax.set_xlabel("Walking speed (m/s)")
    ax.set_ylabel("Car speed (m/s)")
    ax.set_title("Estimated pedestrian crossing margin of safety (s)")
    for i, row in enumerate(data):
        for j, m in enumerate(row):
            ax.text(j, i, f"{m:.2f}", ha="center", va="center", fontsize=9)
    fig.colorbar(im, ax=ax, label="seconds")
    fig.tight_layout()
    fig.savefig(OUT / "margin_of_safety_heatmap.png", dpi=150)
    print(f"Saved plot to {OUT / 'margin_of_safety_heatmap.png'}")


if __name__ == "__main__":
    main()
