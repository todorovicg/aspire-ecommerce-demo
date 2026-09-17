import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { hueFor, initialsFor } from './product-image-palette';

@Component({
  selector: 'app-product-image',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './product-image.html',
  styleUrl: './product-image.scss',
})
export class ProductImage {
  readonly imageKey = input.required<string>();
  readonly name = input.required<string>();

  protected readonly background = computed(() => `hsl(${hueFor(this.imageKey())} 45% 55%)`);
  protected readonly initials = computed(() => initialsFor(this.name()));
}
