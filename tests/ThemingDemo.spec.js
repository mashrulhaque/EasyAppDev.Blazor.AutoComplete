// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Theming Demo - Size Variants', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5678/theming-demo');
    await page.waitForLoadState('networkidle');
  });

  test('should change component size when clicking size buttons', async ({ page }) => {
    // Find the first preview component (Simple Mode)
    const firstInput = page.locator('.ebd-ac-input').first();

    // Test Default size (initial state)
    await expect(firstInput).toHaveClass(/ebd-ac-size-default/);

    // Click Compact button
    await page.getByRole('button', { name: /Compact.*25% smaller spacing/i }).click();
    await page.waitForTimeout(500); // Wait for transitions

    // Verify the size class changed to compact
    const containerCompact = page.locator('.ebd-ac-container').first();
    await expect(containerCompact).toHaveClass(/ebd-ac-size-compact/);

    // Check computed styles for compact size
    const inputCompact = page.locator('.ebd-ac-input').first();
    const fontSizeCompact = await inputCompact.evaluate((el) =>
      window.getComputedStyle(el).fontSize
    );
    expect(fontSizeCompact).toBe('13px'); // Compact size font

    // Click Large button
    await page.getByRole('button', { name: /Large.*25% larger spacing/i }).click();
    await page.waitForTimeout(500); // Wait for transitions

    // Verify the size class changed to large
    const containerLarge = page.locator('.ebd-ac-container').first();
    await expect(containerLarge).toHaveClass(/ebd-ac-size-large/);

    // Check computed styles for large size
    const inputLarge = page.locator('.ebd-ac-input').first();
    const fontSizeLarge = await inputLarge.evaluate((el) =>
      window.getComputedStyle(el).fontSize
    );
    expect(fontSizeLarge).toBe('16px'); // Large size font

    // Click Default button
    await page.getByRole('button', { name: /Default.*Standard spacing/i }).click();
    await page.waitForTimeout(500); // Wait for transitions

    // Verify the size class changed back to default
    const containerDefault = page.locator('.ebd-ac-container').first();
    await expect(containerDefault).toHaveClass(/ebd-ac-size-default/);

    // Check computed styles for default size
    const inputDefault = page.locator('.ebd-ac-input').first();
    const fontSizeDefault = await inputDefault.evaluate((el) =>
      window.getComputedStyle(el).fontSize
    );
    expect(fontSizeDefault).toBe('14px'); // Default size font
  });

  test('should update all preview components when changing size', async ({ page }) => {
    // Get all preview component containers
    const containers = page.locator('.ebd-ac-container');
    const containerCount = await containers.count();
    expect(containerCount).toBeGreaterThanOrEqual(4); // Should have 4 preview components

    // Click Compact button
    await page.getByRole('button', { name: /Compact.*25% smaller spacing/i }).click();
    await page.waitForTimeout(500);

    // Verify all preview containers have compact class
    for (let i = 0; i < 4; i++) {
      await expect(containers.nth(i)).toHaveClass(/ebd-ac-size-compact/);
    }

    // Click Large button
    await page.getByRole('button', { name: /Large.*25% larger spacing/i }).click();
    await page.waitForTimeout(500);

    // Verify all preview containers have large class
    for (let i = 0; i < 4; i++) {
      await expect(containers.nth(i)).toHaveClass(/ebd-ac-size-large/);
    }
  });

  test('should highlight active size button', async ({ page }) => {
    const compactBtn = page.getByRole('button', { name: /Compact.*25% smaller spacing/i });
    const defaultBtn = page.getByRole('button', { name: /Default.*Standard spacing/i });
    const largeBtn = page.getByRole('button', { name: /Large.*25% larger spacing/i });

    // Initially Default should be active (btn-success)
    await expect(defaultBtn).toHaveClass(/btn-success/);
    await expect(compactBtn).toHaveClass(/btn-outline-success/);
    await expect(largeBtn).toHaveClass(/btn-outline-success/);

    // Click Compact
    await compactBtn.click();
    await page.waitForTimeout(300);

    // Compact should be active
    await expect(compactBtn).toHaveClass(/btn-success/);
    await expect(defaultBtn).toHaveClass(/btn-outline-success/);
    await expect(largeBtn).toHaveClass(/btn-outline-success/);

    // Click Large
    await largeBtn.click();
    await page.waitForTimeout(300);

    // Large should be active
    await expect(largeBtn).toHaveClass(/btn-success/);
    await expect(compactBtn).toHaveClass(/btn-outline-success/);
    await expect(defaultBtn).toHaveClass(/btn-outline-success/);
  });

  test('should respect size when no custom properties are set', async ({ page }) => {
    // Reset all custom properties first
    await page.getByRole('button', { name: 'Reset' }).nth(0).click(); // Primary color reset
    await page.getByRole('button', { name: 'Reset' }).nth(1).click(); // Background color reset
    await page.getByRole('button', { name: 'Reset' }).nth(2).click(); // Text color reset
    await page.getByRole('button', { name: 'Reset' }).nth(3).click(); // Border radius reset
    await page.getByRole('button', { name: 'Reset' }).nth(4).click(); // Font size reset
    await page.getByRole('button', { name: 'Reset' }).nth(5).click(); // Font family reset

    await page.waitForTimeout(500);

    // Now test size changes work properly
    const input = page.locator('.ebd-ac-input').first();

    // Test Compact
    await page.getByRole('button', { name: /Compact/i }).click();
    await page.waitForTimeout(500);
    let fontSize = await input.evaluate((el) => window.getComputedStyle(el).fontSize);
    expect(fontSize).toBe('13px');

    // Test Large
    await page.getByRole('button', { name: /Large/i }).click();
    await page.waitForTimeout(500);
    fontSize = await input.evaluate((el) => window.getComputedStyle(el).fontSize);
    expect(fontSize).toBe('16px');

    // Test Default
    await page.getByRole('button', { name: /Default.*Standard/i }).click();
    await page.waitForTimeout(500);
    fontSize = await input.evaluate((el) => window.getComputedStyle(el).fontSize);
    expect(fontSize).toBe('14px');
  });

  test('side-by-side comparison shows different sizes', async ({ page }) => {
    // Find the side-by-side comparison section
    const compactPreview = page.locator('text=Compact').nth(1).locator('..').locator('.ebd-ac-input');
    const defaultPreview = page.locator('text=Default').nth(1).locator('..').locator('.ebd-ac-input');
    const largePreview = page.locator('text=Large').nth(1).locator('..').locator('.ebd-ac-input');

    // Get font sizes
    const compactFontSize = await compactPreview.evaluate((el) =>
      window.getComputedStyle(el).fontSize
    );
    const defaultFontSize = await defaultPreview.evaluate((el) =>
      window.getComputedStyle(el).fontSize
    );
    const largeFontSize = await largePreview.evaluate((el) =>
      window.getComputedStyle(el).fontSize
    );

    // Verify they are different
    expect(compactFontSize).toBe('13px');
    expect(defaultFontSize).toBe('14px');
    expect(largeFontSize).toBe('16px');

    // Verify padding is different too
    const compactPadding = await compactPreview.evaluate((el) =>
      window.getComputedStyle(el).padding
    );
    const defaultPadding = await defaultPreview.evaluate((el) =>
      window.getComputedStyle(el).padding
    );
    const largePadding = await largePreview.evaluate((el) =>
      window.getComputedStyle(el).padding
    );

    expect(compactPadding).not.toBe(defaultPadding);
    expect(defaultPadding).not.toBe(largePadding);
  });
});
