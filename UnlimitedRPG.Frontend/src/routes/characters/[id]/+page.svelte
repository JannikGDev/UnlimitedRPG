<script lang="ts">
	import { getCharacter } from '$lib/api';
	import type { CharacterDto } from '$lib/api';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	let character = $state<CharacterDto | null>(null);
	let error = $state('');

	$effect(() => {
		getCharacter(data.id)
			.then((c) => (character = c))
			.catch(() => (error = 'Character not found.'));
	});
</script>

<main class="mx-auto max-w-2xl px-4 py-12 space-y-6">
	<a href="/" class="text-sm hover:underline">← Back</a>

	{#if error}
		<p class="text-sm text-red-500">{error}</p>
	{:else if !character}
		<p class="text-sm text-gray-500">Loading…</p>
	{:else}
		<h1 class="text-3xl font-bold">{character.name}</h1>
		<p class="text-gray-700">{character.description}</p>
	{/if}
</main>
