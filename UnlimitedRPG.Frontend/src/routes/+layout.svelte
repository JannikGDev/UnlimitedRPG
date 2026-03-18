<script lang="ts">
	import '../app.css';
	import favicon from '$lib/assets/favicon.svg';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
	<title>UnlimitedRPG</title>
</svelte:head>

<div class="min-h-screen bg-zinc-950 text-zinc-100 font-sans">
	{@render children()}
</div>
