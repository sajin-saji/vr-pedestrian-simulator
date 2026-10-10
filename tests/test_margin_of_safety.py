import importlib.util
from pathlib import Path
import unittest

MODULE_PATH = Path(__file__).resolve().parents[1] / "analysis" / "margin_of_safety.py"
spec = importlib.util.spec_from_file_location("margin_of_safety", MODULE_PATH)
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


class SafetyMarginTests(unittest.TestCase):
    def test_reference_case(self):
        self.assertAlmostEqual(module.margin(8, 1), 2)

    def test_crossing_time_equal_arrival(self):
        self.assertAlmostEqual(module.margin(10, 1), 0)

    def test_published_table_with_rounding_tolerance(self):
        for car_speed, expected in module.PAPER_TABLE.items():
            for walking_speed, value in zip(module.WALK_SPEEDS, expected):
                with self.subTest(car_speed=car_speed, walking_speed=walking_speed):
                    self.assertLessEqual(abs(module.margin(car_speed, walking_speed) - value), .021)


if __name__ == "__main__":
    unittest.main()
