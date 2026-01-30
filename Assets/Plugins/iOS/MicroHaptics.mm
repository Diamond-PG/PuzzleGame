#import <UIKit/UIKit.h>

extern "C"
{
    bool _MicroHaptics_Haptic(int style)
    {
        if (@available(iOS 10.0, *))
        {
            switch (style)
            {
                case 0: // Selection
                {
                    UISelectionFeedbackGenerator *gen = [[UISelectionFeedbackGenerator alloc] init];
                    [gen prepare];
                    [gen selectionChanged];
                    return true;
                }

                case 1: // Light
                {
                    UIImpactFeedbackGenerator *gen =
                        [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
                    [gen prepare];
                    [gen impactOccurred];
                    return true;
                }

                case 2: // Medium
                {
                    UIImpactFeedbackGenerator *gen =
                        [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
                    [gen prepare];
                    [gen impactOccurred];
                    return true;
                }

                case 3: // Heavy
                {
                    UIImpactFeedbackGenerator *gen =
                        [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
                    [gen prepare];
                    [gen impactOccurred];
                    return true;
                }
            }
        }

        return false;
    }
}