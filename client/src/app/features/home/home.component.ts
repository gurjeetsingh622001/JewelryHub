import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';

interface CategoryTile {
  name: string;
  description: string;
  image: string;
  /** No real Category data is seeded yet — a text search against product name is an honest stand-in for "browse by type" (see NavbarComponent). */
  searchTerm: string;
}

interface FeaturedPiece {
  name: string;
  category: string;
  price: string;
  image: string;
}

// Unsplash CDN images, free tier (Unsplash License — no attribution
// required), resized via their query-param API. See docs/DESIGN_SYSTEM.md
// § Imagery. No product/category data yet — Products/Categories APIs exist
// on the backend but the Customer UI hasn't been wired to them (Phase 13b),
// so this is illustrative content, not live inventory.
@Component({
  selector: 'app-home',
  imports: [RouterLink, LucideAngularModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent {
  protected readonly categories: CategoryTile[] = [
    {
      name: 'Rings',
      description: 'Solitaires, bands & statement pieces',
      image: 'https://images.unsplash.com/photo-1640724390912-4d92d9a985fe?q=80&w=800&auto=format&fit=crop',
      searchTerm: 'ring',
    },
    {
      name: 'Necklaces',
      description: 'Pendants, chains & bridal sets',
      image: 'https://images.unsplash.com/photo-1744369382892-eb5b6a2fdc6f?q=80&w=800&auto=format&fit=crop',
      searchTerm: 'necklace',
    },
    {
      name: 'Earrings',
      description: 'Studs, hoops & drop earrings',
      image: 'https://images.unsplash.com/photo-1605035184674-1ee3fa430b7e?q=80&w=800&auto=format&fit=crop',
      searchTerm: 'earring',
    },
    {
      name: 'Bracelets',
      description: 'Bangles, cuffs & tennis bracelets',
      image: 'https://images.unsplash.com/photo-1731441326417-01b7dbbdc144?q=80&w=800&auto=format&fit=crop',
      searchTerm: 'bracelet',
    },
  ];

  protected readonly featured: FeaturedPiece[] = [
    {
      name: 'Aurelia Diamond Pendant',
      category: 'Necklaces',
      price: '₹48,500',
      image: 'https://images.unsplash.com/photo-1747933509433-c58152c10ee7?q=80&w=900&auto=format&fit=crop',
    },
    {
      name: 'Heritage Gift Edit',
      category: 'Curated Sets',
      price: '₹72,000',
      image: 'https://images.unsplash.com/photo-1769116416641-e714b71851e8?q=80&w=900&auto=format&fit=crop',
    },
    {
      name: 'Bridal Diamond Ring Duo',
      category: 'Bridal Edit',
      price: '₹1,25,000',
      image: 'https://images.unsplash.com/photo-1769116416517-594639a769a7?q=80&w=900&auto=format&fit=crop',
    },
  ];

  protected readonly heroImage =
    'https://images.unsplash.com/photo-1599707367072-cd6ada2bc375?q=80&w=1600&auto=format&fit=crop';
}
